import ChatInterface from "@/components/ChatInterface";

export default function AdminPage() {
    return (<ChatInterface
        userType={"doctor"} // 'patient' or 'doctor'
        userId={6789}
        receiverId={12345}
        userName={"Davis"}
        receiverName={"John"}
      />);
}