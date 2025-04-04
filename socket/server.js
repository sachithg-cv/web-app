// File: server.js
const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const cors = require('cors');
const mongoose = require('mongoose');

// Database connection
// mongoose.connect(process.env.MONGODB_URI || 'mongodb://localhost:27017/medical-chat', {
//   useNewUrlParser: true,
//   useUnifiedTopology: true
// })
// .then(() => console.log('Connected to MongoDB'))
// .catch(err => console.error('MongoDB connection error:', err));

// Message model
// const messageSchema = new mongoose.Schema({
//   senderId: { type: String, required: true },
//   receiverId: { type: String, required: true },
//   content: { type: String, required: true },
//   senderType: { type: String, enum: ['patient', 'doctor'], required: true },
//   timestamp: { type: Date, default: Date.now },
//   read: { type: Boolean, default: false }
// });

// const Message = mongoose.model('Message', messageSchema);

// Express app setup
const app = express();
app.use(cors());
app.use(express.json());

// Simple health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({ status: 'ok' });
});

// Create HTTP server
const server = http.createServer(app);

// Socket.io setup
const io = new Server(server, {
  cors: {
    origin: process.env.FRONTEND_URL || 'http://localhost:3000',
    methods: ['GET', 'POST']
  }
});

// Store active users
const activeUsers = new Map();

// Socket.io connection handler
io.on('connection', (socket) => {
  const { userId, userType } = socket.handshake.query;
  
  console.log(`User connected: ${userId} (${userType})`);
  activeUsers.set(userId, { socketId: socket.id, userType });
  
  // Join a private room for chat
  socket.on('joinRoom', async ({ userId, receiverId }) => {
    const roomId = [userId, receiverId].sort().join('-');
    
    socket.join(roomId);
    console.log(`User ${userId} joined room ${roomId}`);
    
    // Fetch message history from database
    // try {
    //   const messages = await Message.find({
    //     $or: [
    //       { senderId: userId, receiverId: receiverId },
    //       { senderId: receiverId, receiverId: userId }
    //     ]
    //   }).sort({ timestamp: 1 });
      
    //   socket.emit('messageHistory', messages);
      
    //   // Mark messages as read
    //   await Message.updateMany(
    //     { senderId: receiverId, receiverId: userId, read: false },
    //     { $set: { read: true } }
    //   );
    // } catch (error) {
    //   console.error('Error fetching message history:', error);
    //   socket.emit('error', { message: 'Failed to load message history' });
    // }
  });
  
  // Send a message
  socket.on('sendMessage', async (messageData) => {
    const roomId = [messageData.senderId, messageData.receiverId].sort().join('-');
    
    try {
      // Create and save message
      const message = {
        senderId: messageData.senderId,
        receiverId: messageData.receiverId,
        content: messageData.content,
        senderType: messageData.senderType,
        timestamp: new Date(),
        read: false
      };
      
      //const savedMessage = await message.save();
      
      // Broadcast message to room
      io.to(roomId).emit('newMessage', message);
      
      // Send notification to receiver if they're not in the room
      const receiverSocketId = activeUsers.get(messageData.receiverId)?.socketId;
      if (receiverSocketId) {
        io.to(receiverSocketId).emit('messageNotification', {
          senderId: messageData.senderId,
          senderType: messageData.senderType,
          message: messageData.content
        });
      }
    } catch (error) {
      console.error('Error saving message:', error);
      socket.emit('error', { message: 'Failed to send message' });
    }
  });
  
  // Typing indicator
  socket.on('typing', ({ userId, receiverId, isTyping }) => {
    const roomId = [userId, receiverId].sort().join('-');
    socket.to(roomId).emit('userTyping', { isTyping });
  });
  
  // Mark messages as read
//   socket.on('markAsRead', async ({ userId, receiverId }) => {
//     try {
//       await Message.updateMany(
//         { senderId: receiverId, receiverId: userId, read: false },
//         { $set: { read: true } }
//       );
      
//       const roomId = [userId, receiverId].sort().join('-');
//       io.to(roomId).emit('messagesRead', { userId });
//     } catch (error) {
//       console.error('Error marking messages as read:', error);
//     }
//   });
  
  // Disconnect
  socket.on('disconnect', () => {
    activeUsers.delete(userId);
    console.log(`User disconnected: ${userId}`);
  });
});

// Start server
const PORT = process.env.PORT || 3001;
server.listen(PORT, () => {
  console.log(`Socket.io server running on port ${PORT}`);
});

// API routes for chat management
// GET endpoint to fetch all chats for a user
// app.get('/api/chats/:userId', async (req, res) => {
//   try {
//     const userId = req.params.userId;
    
//     // Find all unique conversations for this user
//     const sentMessages = await Message.find({ senderId: userId })
//       .distinct('receiverId');
      
//     const receivedMessages = await Message.find({ receiverId: userId })
//       .distinct('senderId');
      
//     // Combine unique IDs
//     const uniqueUserIds = [...new Set([...sentMessages, ...receivedMessages])];
    
//     // For each unique user, get the last message and unread count
//     const chats = await Promise.all(uniqueUserIds.map(async (otherUserId) => {
//       // Get last message
//       const lastMessage = await Message.findOne({
//         $or: [
//           { senderId: userId, receiverId: otherUserId },
//           { senderId: otherUserId, receiverId: userId }
//         ]
//       }).sort({ timestamp: -1 });
      
//       // Count unread messages
//       const unreadCount = await Message.countDocuments({
//         senderId: otherUserId,
//         receiverId: userId,
//         read: false
//       });
      
//       return {
//         userId: otherUserId,
//         lastMessage,
//         unreadCount
//       };
//     }));
    
//     res.json(chats);
//   } catch (error) {
//     console.error('Error fetching chats:', error);
//     res.status(500).json({ error: 'Failed to fetch chats' });
//   }
// });

module.exports = { app, server };