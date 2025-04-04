"use client"

import React, { useState, useEffect, useRef } from 'react';
import io from 'socket.io-client';

const ChatInterface = ({ userType, userId, receiverId, userName, receiverName }) => {
  const [messages, setMessages] = useState([]);
  const [socket, setSocket] = useState(null);
  const [isConnected, setIsConnected] = useState(false);
  const [isTyping, setIsTyping] = useState(false);
  const [inputMessage, setInputMessage] = useState('');
  const messagesEndRef = useRef(null);
  
  // Store message IDs that we've already processed
  const [processedMessageIds] = useState(new Set());
  
  // Connect to socket server when component mounts
  useEffect(() => {
    // Initialize socket connection
    const socketInit = io(process.env.NEXT_PUBLIC_SOCKET_URL || 'http://localhost:3001', {
      query: {
        userId,
        userType // 'patient' or 'doctor'
      }
    });
    
    setSocket(socketInit);
    
    // Cleanup on unmount
    return () => {
      if (socketInit) socketInit.disconnect();
    };
  }, [userId, userType]);
  
  // Set up socket event listeners
  useEffect(() => {
    if (!socket) return;
    
    // Connection established
    socket.on('connect', () => {
      setIsConnected(true);
      console.log('Connected to socket server');
      
      // Load message history
      socket.emit('joinRoom', { userId, receiverId });
    });
    
    // Connection lost
    socket.on('disconnect', () => {
      setIsConnected(false);
      console.log('Disconnected from socket server');
    });
    
    // Receive message history
    socket.on('messageHistory', (history) => {
      // Add all history message IDs to our processed set
      history.forEach(msg => {
        if (msg.id) processedMessageIds.add(msg.id);
      });
      
      setMessages(history);
    });
    
    // Receive new message - only from others
    socket.on('newMessage', (message) => {
      // Skip if this is our own message or if we've already processed it
      if (message.senderId === userId || (message.id && processedMessageIds.has(message.id))) {
        return;
      }
      
      // Add this message ID to our processed set
      if (message.id) processedMessageIds.add(message.id);
      
      setMessages(prev => [...prev, message]);
    });
    
    // Confirmation that our message was received by the server
    socket.on('messageSent', (confirmedMessage) => {
      // Replace our local version with the server version if needed
      setMessages(prev => {
        const messageExists = prev.some(m => 
          m.id === confirmedMessage.id ||
          (m.tempId && m.tempId === confirmedMessage.tempId)
        );
        
        if (messageExists) {
          // Update existing message
          return prev.map(m => 
            (m.id === confirmedMessage.id || (m.tempId && m.tempId === confirmedMessage.tempId))
              ? confirmedMessage
              : m
          );
        } else {
          // If it doesn't exist yet (rare case), add it
          return [...prev, confirmedMessage];
        }
      });
    });
    
    // User is typing
    socket.on('userTyping', ({ isTyping }) => {
      setIsTyping(isTyping);
    });
    
    // Clear listeners on cleanup
    return () => {
      socket.off('connect');
      socket.off('disconnect');
      socket.off('messageHistory');
      socket.off('newMessage');
      socket.off('messageSent');
      socket.off('userTyping');
    };
  }, [socket, userId, receiverId, processedMessageIds]);
  
  // Auto-scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);
  
  // Handle sending a message
  const sendMessage = (e) => {
    e.preventDefault();
    
    if (!socket || !isConnected || !inputMessage.trim()) return;
    
    // Create a temporary ID to track this message
    const tempId = `temp-${Date.now()}`;
    
    const message = {
      tempId, // Include tempId for tracking
      senderId: userId,
      receiverId,
      content: inputMessage,
      senderType: userType,
      timestamp: new Date().toISOString(),
      pending: true // Mark as pending until confirmed
    };
    
    // Add to local messages immediately (optimistic UI update)
    setMessages(prev => [...prev, message]);
    
    // Send to server - we don't need to handle the actual message response
    // in the emit handler since we've set up a listener for 'messageSent' above
    socket.emit('sendMessage', {
      tempId, // Pass tempId to server so it can return it
      senderId: userId,
      receiverId,
      content: inputMessage,
      senderType: userType
    });
    
    // Clear input
    setInputMessage('');
  };
  
  // Handle typing indicator
  const handleTyping = (e) => {
    const currentlyTyping = e.target.value.length > 0;
    
    if (!socket || !isConnected) return;
    
    // Only emit typing event if state changes
    if (currentlyTyping !== isTyping) {
      socket.emit('typing', { userId, receiverId, isTyping: currentlyTyping });
      setIsTyping(currentlyTyping);
    }
    
    setInputMessage(e.target.value);
  };
  
  return (
    <div className="flex flex-col h-full bg-gray-50 rounded-lg shadow-md">
      <div className="px-4 py-3 bg-white border-b border-gray-200 rounded-t-lg">
        <h2 className="text-lg font-semibold">{receiverName}</h2>
        <div className="text-sm text-gray-500">
          {isConnected ? (
            <span className="text-green-500">Online</span>
          ) : (
            <span className="text-gray-500">Offline</span>
          )}
          {/* {isTyping && <span className="ml-2 italic">typing...</span>} */}
        </div>
      </div>
      
      <div className="flex-1 overflow-y-auto p-4">
        {messages.length === 0 && (
          <div className="flex items-center justify-center h-full text-gray-500">
            No messages yet. Start the conversation!
          </div>
        )}
        
        {messages.map((message, index) => {
          const isOwnMessage = message.senderId === userId;
          
          return (
            <div 
              key={message.id || message.tempId || index}
              className={`mb-4 ${isOwnMessage ? 'ml-auto max-w-3/4 text-right' : 'mr-auto max-w-3/4'}`}
            >
              <div 
                className={`inline-block rounded-lg px-4 py-2 ${
                  isOwnMessage 
                    ? 'bg-blue-500 text-white' 
                    : message.senderType === 'doctor' 
                      ? 'bg-green-100 text-gray-800' 
                      : 'bg-gray-200 text-gray-800'
                } ${message.pending ? 'opacity-70' : ''}`}
              >
                {message.content}
              </div>
              <div className="text-xs text-gray-500 mt-1">
                {new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                {/* {message.pending && <span className="ml-1">(sending...)</span>} */}
              </div>
            </div>
          );
        })}
        <div ref={messagesEndRef} />
      </div>
      
      <form 
        onSubmit={sendMessage}
        className="p-3 border-t border-gray-200 bg-white rounded-b-lg"
      >
        <div className="flex items-center">
          <input
            type="text"
            value={inputMessage}
            onChange={handleTyping}
            placeholder="Type your message..."
            disabled={!isConnected}
            className="flex-1 py-2 px-3 border border-gray-300 rounded-l-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <button
            type="submit"
            disabled={!inputMessage.trim() || !isConnected}
            className="bg-blue-500 text-white py-2 px-4 rounded-r-lg hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
          >
            Send
          </button>
        </div>
      </form>
    </div>
  );
};

export default ChatInterface;