document.addEventListener('DOMContentLoaded', () => {
    // DOM elements
    const chatButton = document.getElementById('chat-button');
    const chatWindow = document.getElementById('chat-window');
    const minimizeChat = document.getElementById('minimize-chat');
    const messageInput = document.getElementById('message-input');
    const sendButton = document.getElementById('send-button');
    const chatMessages = document.getElementById('chat-messages');
    const typingIndicator = document.getElementById('typing-indicator');
    
    let sessionId = localStorage.getItem('chatSessionId');
    
    // If no session ID exists, generate one and store it
    if (!sessionId) {
        sessionId = generateHashString();
        localStorage.setItem('chatSessionId', sessionId);
        console.log("Generated new session ID:", sessionId);
    } else {
        console.log("Using existing session ID from localStorage:", sessionId);
    }

    
    // Generate a random session ID or get from local storage
    // const sessionId = localStorage.getItem('chatSessionId') || generateSessionId();
    // localStorage.setItem('chatSessionId', sessionId);
    
    // SignalR connection
    let connection = null;
    
    // Toggle chat window
    chatButton.addEventListener('click', () => {
        chatButton.classList.toggle('active');
        chatWindow.classList.toggle('active');
        
        // Initialize connection the first time chat is opened
        // if (chatButton.classList.contains('active') && !connection) {
        //     initializeSignalRConnection();
        // }
        fetchPreviousMessages();
        
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
    
    function generateHashString() {
        // Create a random string with timestamp for uniqueness
        const timestamp = new Date().getTime();
        const randomNum = Math.floor(Math.random() * 1000000);
        const baseString = `${timestamp}-${randomNum}`;
        
        // Simple hash function
        let hash = 0;
        for (let i = 0; i < baseString.length; i++) {
            const char = baseString.charCodeAt(i);
            hash = ((hash << 5) - hash) + char;
            hash = hash & hash; // Convert to 32bit integer
        }
        
        // Convert to hex string and ensure it's positive
        return Math.abs(hash).toString(16);
    }

    function fetchPreviousMessages() {
        // First request to get sessionId
        var chatSettings = {
            "url": `http://localhost:5145/api/chat?sessionId=${sessionId}`,
            "method": "GET",
            "timeout": 0,
            "headers": {
                "Content-Type": "application/json"
            },
            "data": "\r\n",
        };
        
        // Using promise to handle sequential requests
        $.ajax(chatSettings)
            .done(function(response) {
                console.log("Session response:", response);
                
                if (response && response.length > 0) {
                    response.forEach(element => {
                        addMessageToChat(element.sender, element.message);
                    });
                }
            });
            
    }
    // Initialize SignalR connection
    // function initializeSignalRConnection() {
    //     // Create the connection
    //     connection = new signalR.HubConnectionBuilder()
    //         .withUrl("/chatHub")
    //         .withAutomaticReconnect()
    //         .build();
        
    //     // Start the connection
    //     connection.start()
    //         .then(() => {
    //             console.log("SignalR Connected!");
                
    //             // Join the chat session
    //             connection.invoke("JoinSession", sessionId)
    //                 .catch(err => console.error("Error joining session:", err));
    //         })
    //         .catch(err => console.error("Error connecting:", err));
        
    //     // Handle received messages
    //     connection.on("ReceiveMessage", (role, content) => {
    //         addMessageToChat(role, content);
            
    //         // Hide typing indicator
    //         typingIndicator.style.display = 'none';
            
    //         // Scroll to bottom
    //         scrollToBottom();
            
    //         // Enable input
    //         enableUserInput();
    //     });
    // }
    
    // Send message function
    function sendMessage() {
        const message = messageInput.value.trim();
        
        if (message) {
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
                  "message": message,
                  "sessionId": sessionId
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