"use client"

import type React from "react"

import { useState } from "react"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { PaperclipIcon, SendIcon, SmileIcon } from "lucide-react"

type Contact = {
  id: string
  name: string
  role: string
  avatar: string
  lastMessage: string
  time: string
  unread: boolean
}

type Message = {
  id: string
  content: string
  timestamp: string
  sender: "user" | "contact"
}

const contacts: Contact[] = [
  {
    id: "1",
    name: "Dr. Sarah Johnson",
    role: "Primary Care",
    avatar: "/placeholder.svg?height=40&width=40",
    lastMessage: "Your lab results look good. We'll discuss at your next appointment.",
    time: "2h ago",
    unread: true,
  },
  {
    id: "2",
    name: "Pharmacy",
    role: "Medication Services",
    avatar: "/placeholder.svg?height=40&width=40",
    lastMessage: "Your prescription is ready for pickup.",
    time: "Yesterday",
    unread: true,
  },
  {
    id: "3",
    name: "Billing Department",
    role: "Financial Services",
    avatar: "/placeholder.svg?height=40&width=40",
    lastMessage: "Your insurance claim has been processed.",
    time: "2d ago",
    unread: true,
  },
  {
    id: "4",
    name: "Dr. Michael Chen",
    role: "Dermatology",
    avatar: "/placeholder.svg?height=40&width=40",
    lastMessage: "Please send photos of the affected area before your appointment.",
    time: "3d ago",
    unread: false,
  },
  {
    id: "5",
    name: "Nurse Practitioner Emily",
    role: "Primary Care Team",
    avatar: "/placeholder.svg?height=40&width=40",
    lastMessage: "How are you feeling after starting the new medication?",
    time: "1w ago",
    unread: false,
  },
]

const conversationHistory: Record<string, Message[]> = {
  "1": [
    {
      id: "m1",
      content: "Hello Jane, I've reviewed your recent blood work results.",
      timestamp: "2:30 PM",
      sender: "contact",
    },
    {
      id: "m2",
      content: "Hi Dr. Johnson, thank you for checking. Is everything okay?",
      timestamp: "2:35 PM",
      sender: "user",
    },
    {
      id: "m3",
      content: "Yes, your lab results look good. We'll discuss the details at your next appointment on April 10th.",
      timestamp: "2:40 PM",
      sender: "contact",
    },
    {
      id: "m4",
      content: "That's a relief! Is there anything I should do or monitor until then?",
      timestamp: "2:45 PM",
      sender: "user",
    },
    {
      id: "m5",
      content:
        "Just continue with your current medications and diet. If you experience any unusual symptoms, please let me know right away.",
      timestamp: "2:50 PM",
      sender: "contact",
    },
  ],
  "2": [
    {
      id: "m1",
      content: "Hello Jane, your prescription for Lisinopril has been refilled and is ready for pickup.",
      timestamp: "Yesterday, 10:15 AM",
      sender: "contact",
    },
    {
      id: "m2",
      content: "Thank you! What are your hours today?",
      timestamp: "Yesterday, 11:30 AM",
      sender: "user",
    },
    {
      id: "m3",
      content: "We're open until 9 PM today. You can also use the drive-through window if you prefer.",
      timestamp: "Yesterday, 11:45 AM",
      sender: "contact",
    },
  ],
}

export default function MessagesPage() {
  const [activeContact, setActiveContact] = useState<Contact | null>(contacts[0])
  const [newMessage, setNewMessage] = useState("")
  const [messages, setMessages] = useState<Record<string, Message[]>>(conversationHistory)

  const handleSendMessage = () => {
    if (!newMessage.trim() || !activeContact) return

    const newMsg: Message = {
      id: `new-${Date.now()}`,
      content: newMessage,
      timestamp: "Just now",
      sender: "user",
    }

    setMessages((prev) => ({
      ...prev,
      [activeContact.id]: [...(prev[activeContact.id] || []), newMsg],
    }))

    setNewMessage("")
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault()
      handleSendMessage()
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Messages</h1>
        <p className="text-muted-foreground">Communicate with your healthcare team</p>
      </div>

      <Tabs defaultValue="inbox" className="w-full">
        <TabsList className="grid w-full grid-cols-2 md:w-[400px]">
          <TabsTrigger value="inbox">Inbox</TabsTrigger>
          <TabsTrigger value="sent">Sent</TabsTrigger>
        </TabsList>
        <TabsContent value="inbox" className="pt-4">
          <div className="grid h-[calc(100vh-220px)] grid-cols-1 gap-4 md:grid-cols-[300px_1fr]">
            <Card className="h-full">
              <CardHeader className="px-4 py-3">
                <CardTitle className="text-lg">Conversations</CardTitle>
              </CardHeader>
              <CardContent className="p-0">
                <ScrollArea className="h-[calc(100vh-300px)]">
                  <div className="space-y-1 p-2">
                    {contacts.map((contact) => (
                      <button
                        key={contact.id}
                        className={`flex w-full items-start gap-3 rounded-md p-2 text-left ${
                          activeContact?.id === contact.id ? "bg-primary text-primary-foreground" : "hover:bg-muted"
                        }`}
                        onClick={() => setActiveContact(contact)}
                      >
                        <Avatar className="h-10 w-10 border">
                          <AvatarImage src={contact.avatar} alt={contact.name} />
                          <AvatarFallback>{contact.name[0]}</AvatarFallback>
                        </Avatar>
                        <div className="flex-1 overflow-hidden">
                          <div className="flex items-center justify-between">
                            <p className="font-medium">{contact.name}</p>
                            <p className="text-xs">{contact.time}</p>
                          </div>
                          <p className="text-xs text-muted-foreground">{contact.role}</p>
                          <p className="truncate text-xs">{contact.lastMessage}</p>
                          {contact.unread && (
                            <div className="mt-1 flex items-center">
                              <div className="h-2 w-2 rounded-full bg-primary"></div>
                              <span className="ml-1 text-xs font-medium">New</span>
                            </div>
                          )}
                        </div>
                      </button>
                    ))}
                  </div>
                </ScrollArea>
              </CardContent>
            </Card>

            {activeContact ? (
              <Card className="flex flex-col h-full">
                <CardHeader className="flex flex-row items-center gap-3 px-4 py-3">
                  <Avatar className="h-10 w-10 border">
                    <AvatarImage src={activeContact.avatar} alt={activeContact.name} />
                    <AvatarFallback>{activeContact.name[0]}</AvatarFallback>
                  </Avatar>
                  <div>
                    <CardTitle className="text-lg">{activeContact.name}</CardTitle>
                    <p className="text-sm text-muted-foreground">{activeContact.role}</p>
                  </div>
                </CardHeader>
                <CardContent className="flex-1 p-0">
                  <ScrollArea className="h-[calc(100vh-400px)] p-4">
                    <div className="space-y-4">
                      {messages[activeContact.id]?.map((message) => (
                        <div
                          key={message.id}
                          className={`flex ${message.sender === "user" ? "justify-end" : "justify-start"}`}
                        >
                          <div
                            className={`max-w-[80%] rounded-lg p-3 ${
                              message.sender === "user" ? "bg-primary text-primary-foreground" : "bg-muted"
                            }`}
                          >
                            <p className="text-sm">{message.content}</p>
                            <p className="mt-1 text-right text-xs text-muted-foreground">{message.timestamp}</p>
                          </div>
                        </div>
                      ))}
                    </div>
                  </ScrollArea>
                </CardContent>
                <div className="border-t p-4">
                  <div className="flex items-end gap-2">
                    <Button variant="outline" size="icon" className="rounded-full h-9 w-9">
                      <PaperclipIcon className="h-4 w-4" />
                      <span className="sr-only">Attach file</span>
                    </Button>
                    <div className="relative flex-1">
                      <Input
                        placeholder="Type your message..."
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        onKeyDown={handleKeyDown}
                        className="pr-10"
                      />
                      <Button variant="ghost" size="icon" className="absolute right-0 top-0 h-full rounded-l-none">
                        <SmileIcon className="h-4 w-4" />
                        <span className="sr-only">Add emoji</span>
                      </Button>
                    </div>
                    <Button
                      size="icon"
                      className="rounded-full h-9 w-9"
                      onClick={handleSendMessage}
                      disabled={!newMessage.trim()}
                    >
                      <SendIcon className="h-4 w-4" />
                      <span className="sr-only">Send message</span>
                    </Button>
                  </div>
                </div>
              </Card>
            ) : (
              <Card className="flex h-full items-center justify-center">
                <div className="text-center">
                  <p className="text-lg font-medium">Select a conversation</p>
                  <p className="text-sm text-muted-foreground">Choose a contact to view your conversation history</p>
                </div>
              </Card>
            )}
          </div>
        </TabsContent>
        <TabsContent value="sent" className="pt-4">
          <Card className="h-[calc(100vh-220px)]">
            <CardHeader>
              <CardTitle>Sent Messages</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-center py-8">
                <p className="text-muted-foreground">Your sent messages will appear here</p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

