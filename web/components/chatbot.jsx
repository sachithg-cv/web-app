"use client"

import React, { useEffect, useState } from "react";
import "../styles/chatbot.css"; 
import { IoIosSend } from "react-icons/io";
import { FaCommentMedical, FaTimes } from "react-icons/fa";

const Chatbot = () => {

    const generateSessionId = () => {
        const timestamp = Date.now();
        const randomNum = Math.floor(Math.random() * 1000000);
        const str = `${timestamp}-${randomNum}`;
        let hash = 0;
        for (let i = 0; i < str.length; i++) {
            hash = ((hash << 5) - hash) + str.charCodeAt(i);
            hash |= 0;
        }
        return Math.abs(hash).toString(16);
    };

    const [visible, setVisible] = useState(false);
    const [messages, setMessages] = useState([]);
    const [input, setInput] = useState("");
    const [typing, setTyping] = useState(false);
    const [sessionId, setSessionId] = useState(() => {
        // Check if window is defined (client-side) before accessing localStorage
        if (typeof window !== 'undefined') {
            return localStorage.getItem("chatSessionId") || generateSessionId();
        }
        return generateSessionId();
    });

    useEffect(() => {
        // Only execute localStorage operations on the client side
        if (typeof window !== 'undefined') {
            if (!localStorage.getItem("chatSessionId")) {
                localStorage.setItem("chatSessionId", sessionId);
            }
            fetchPreviousMessages();
        }
    }, [sessionId]);

    const fetchPreviousMessages = async () => {
        try {
            const response = await fetch(`http://localhost:5145/api/chat?sessionId=${sessionId}`, {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const data = await response.json();

            if (data && data.length > 0) {
                setMessages(data.map(msg => ({ role: msg.sender, content: msg.message })));
                scrollToBottom();
            }
        } catch (error) {
            console.error('Error fetching messages:', error);
        }
    };
    
    const sendMessage = async () => {
        const trimmed = input.trim();
        if (!trimmed) return;

        setMessages(prev => [...prev, { role: "user", content: trimmed }]);
        setInput("");
        setTyping(true);
        scrollToBottom();

        try {
            const response = await fetch("http://localhost:5145/api/chat", {
                method: "POST",
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    message: trimmed,
                    sessionId: sessionId
                })
            });

            const data = await response.text(); // or use `.json()` if your API returns JSON
            setMessages(prev => [...prev, { role: "system", content: data }]);
        } catch (error) {
            console.error("Error sending message:", error);
        } finally {
            setTyping(false);
            scrollToBottom();
        }
    };

    const scrollToBottom = () => {
        setTimeout(() => {
            const chatMessages = document.querySelector('.chat-messages');
            if (chatMessages) {
                chatMessages.scrollTop = chatMessages.scrollHeight;
            }
        }, 100);
    };

    return (
        <div id="chat-container">
            <div className={`chat-window ${visible ? "active" : ""}`}>
                <div className="chat-header">
                    <h5>Medical Appointment Assistant</h5>
                    <button
                        className="btn-close"
                        aria-label="Close"
                        onClick={() => setVisible(false)}
                    ></button>
                </div>
                <div className="chat-messages">
                    {messages.map((msg, index) => (
                        <>
                            <div key={index} className={`chat-message ${msg.role === "user" ? "message-user" : "message-system"}`}>
                                {msg.content}
                            </div>
                            <span className="deliver-time">11.30</span>
                        </>
                    ))}
                    {typing && (
                        <div className="typing-indicator" id="typing-indicator">
                            <span className="dot" />
                            <span className="dot" />
                            <span className="dot" />
                        </div>
                    )}
                </div>
                <div className="chat-input">
                    <input
                        type="text"
                        placeholder="Type your message here..."
                        className="form-control"
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        onKeyDown={(e) => e.key === "Enter" && sendMessage()}
                    />
                    <button className={`btn btn-primary ${typing ? "disabled-send-btn":""}`} onClick={sendMessage} disabled={typing} >
                        <IoIosSend size={30} color="#e9f3ff"/>
                    </button>
                </div>
            </div>

            <button
                id="chat-button"
                className={`chat-button ${visible ? "active" : ""}`}
                onClick={() => setVisible(!visible)}
            >
                <FaCommentMedical className="chat-icon" />
                <FaTimes className="close-icon" />
            </button>
        </div>
    );
};

export default Chatbot;
