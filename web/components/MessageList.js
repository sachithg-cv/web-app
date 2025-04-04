"use client"

import React from 'react';
import { formatRelative } from 'date-fns';

const MessageList = ({ messages, currentUserId, messagesEndRef }) => {
  if (!messages.length) {
    return (
      <div className="flex-1 overflow-y-auto p-4 flex items-center justify-center text-gray-500">
        No messages yet. Start the conversation!
      </div>
    );
  }
  
  return (
    <div className="flex-1 overflow-y-auto p-4">
      {messages.map((message, index) => {
        const isOwnMessage = message.senderId === currentUserId;
        
        return (
          <div 
            key={index}
            className={`mb-4 ${isOwnMessage ? 'ml-auto max-w-3/4 text-right' : 'mr-auto max-w-3/4'}`}
          >
            <div 
              className={`inline-block rounded-lg px-4 py-2 ${
                isOwnMessage 
                  ? 'bg-blue-500 text-white' 
                  : message.senderType === 'doctor' 
                    ? 'bg-green-100 text-gray-800' 
                    : 'bg-gray-200 text-gray-800'
              }`}
            >
              {message.content}
            </div>
            <div className="text-xs text-gray-500 mt-1">
              {formatRelative(new Date(message.timestamp), new Date())}
            </div>
          </div>
        );
      })}
      <div ref={messagesEndRef} />
    </div>
  );
};

export default MessageList;