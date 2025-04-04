"use client"

import React, { useState, useEffect } from 'react';

const MessageInput = ({ sendMessage, onTyping, disabled }) => {
  const [message, setMessage] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  
  // Handle typing indicator
  useEffect(() => {
    if (!message && isTyping) {
      setIsTyping(false);
      onTyping(false);
    } else if (message && !isTyping) {
      setIsTyping(true);
      onTyping(true);
    }
    
    // Debounce typing indicator
    const typingTimeout = setTimeout(() => {
      if (isTyping) {
        setIsTyping(false);
        onTyping(false);
      }
    }, 3000);
    
    return () => clearTimeout(typingTimeout);
  }, [message, isTyping, onTyping]);
  
  const handleSubmit = (e) => {
    e.preventDefault();
    if (!message.trim()) return;
    
    sendMessage(message);
    setMessage('');
    setIsTyping(false);
    onTyping(false);
  };
  
  return (
    <form 
      onSubmit={handleSubmit}
      className="p-3 border-t border-gray-200 bg-white rounded-b-lg"
    >
      <div className="flex items-center">
        <input
          type="text"
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          placeholder="Type your message..."
          disabled={disabled}
          className="flex-1 py-2 px-3 border border-gray-300 rounded-l-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          type="submit"
          disabled={!message.trim() || disabled}
          className="bg-blue-500 text-white py-2 px-4 rounded-r-lg hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
        >
          Send
        </button>
      </div>
    </form>
  );
};

export default MessageInput;