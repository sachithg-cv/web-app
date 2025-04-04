document.addEventListener('DOMContentLoaded', () => {
    // DOM elements
    const chatButton = document.getElementById('chat-button');
    const chatWindow = document.getElementById('chat-window');
    const minimizeChat = document.getElementById('minimize-chat');
    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');
    const chatMessages = document.getElementById('chat-messages');
    const typingIndicator = document.getElementById('typing-indicator');

    
    // Generate a random session ID or get from local storage
    const sessionId = localStorage.getItem('chatSessionId') || generateSessionId();
    localStorage.setItem('chatSessionId', sessionId);
    
    // SignalR connection
    let connection = null;
    
    // Toggle chat window
    chatButton.addEventListener('click', () => {
        chatButton.classList.toggle('active');
        chatWindow.classList.toggle('active');
        
        // Initialize connection the first time chat is opened
        if (chatButton.classList.contains('active') && !connection) {
            initializeSignalRConnection();
        }
        
        // Focus on input when chat is opened
        if (chatWindow.classList.contains('active')) {
            messageInput.focus();
        }
    });
    
    // Minimize chat window
    minimizeChat.addEventListener('click', () => {
        chatButton.classList.remove('active');
        chatWindow.classList.remove('active');
    });
    
    // Send message on button click
    sendButton.addEventListener('click', sendMessage);
    
    // Send message on Enter key
    messageInput.addEventListener('keypress', (event) => {
        if (event.key === 'Enter') {
            sendMessage();
        }
    });
    
    // Initialize SignalR connection
    function initializeSignalRConnection() {
        // Create the connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .build();
        
        // Start the connection
        connection.start()
            .then(() => {
                console.log("SignalR Connected!");
                
                // Join the chat session
                connection.invoke("JoinSession", sessionId)
                    .catch(err => console.error("Error joining session:", err));
            })
            .catch(err => console.error("Error connecting:", err));
        
        // Handle received messages
        connection.on("ReceiveMessage", (role, content) => {
            addMessageToChat(role, content);
            
            // Hide typing indicator
            typingIndicator.style.display = 'none';
            
            // Scroll to bottom
            scrollToBottom();
            
            // Enable input
            enableUserInput();
        });
    }
    
    // Send message function
    function sendMessage() {
        const message = messageInput.value.trim();
        
        if (message && connection) {
            // Disable input while processing
            disableUserInput();
            
            // Add user message to chat
            addMessageToChat('user', message);
            
            // Clear input
            messageInput.value = '';
            
            // Show typing indicator
            typingIndicator.style.display = 'flex';
            
            // Scroll to bottom
            scrollToBottom();
            
            // Send message to hub
            var settings = {
                "url": "http://localhost:5145/api/chat",
                "method": "POST",
                "timeout": 0,
                "headers": {
                  "Content-Type": "application/json"
                },
                "data": JSON.stringify({
                  "message": message
                }),
              };
              
              $.ajax(settings).done(function (response) {
                console.log(response);
                addMessageToChat('system', response);
                typingIndicator.style.display = 'none';
                enableUserInput();
              });
        }
    }
    
    // Add message to chat
    function addMessageToChat(role, content) {
        const messageElement = document.createElement('div');
        messageElement.classList.add('message', `message-${role}`);
        
        // Convert URLs to links
        const linkedContent = content.replace(
            /(https?:\/\/[^\s]+)/g, 
            '<a href="$1" target="_blank">$1</a>'
        );
        
        messageElement.innerHTML = linkedContent;
        
        // Insert before typing indicator
        chatMessages.insertBefore(messageElement, typingIndicator);
    }
    
    // Disable user input while processing
    function disableUserInput() {
        messageInput.disabled = true;
        sendButton.disabled = true;
    }
    
    // Enable user input after response
    function enableUserInput() {
        messageInput.disabled = false;
        sendButton.disabled = false;
        messageInput.focus();
    }
    
    // Scroll chat to bottom
    function scrollToBottom() {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    // Generate a random session ID
    function generateSessionId() {
        return 'session-' + Math.random().toString(36).substring(2, 15);
    }
});