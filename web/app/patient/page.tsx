import ChatInterface from '@/components/ChatInterface';

export default function PatientPage() {
    return (<ChatInterface
        userType={"patient"} // 'patient' or 'doctor'
        userId={12345}
        receiverId={6789}
        userName={"John"}
        receiverName={"Davis"}
      />);
}