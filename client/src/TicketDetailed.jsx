import { Outlet, useParams } from "react-router";
import { useState, useEffect } from "react";

export default TicketDetailed;

function TicketDetailed(){
    const [Ticket, SetTicket] = useState([]);
    let params = useParams();
    let ticketId = params.ticketId;
    const [Refresh, SetRefresh] = useState(false);
    
    const [Messages, SetMessages] = useState([]);

    useEffect(()=>{
        fetch(`/api/ticket/${ticketId}`)
            .then(response => response.json())
            .then(data=> {
                console.log(data);
                SetMessages(data.messages);
                SetTicket(data.ticketRecord);
            });
    }, [Refresh]);
    
    function datetimeFormatter(datetime){
        const date = new Date(Date.parse(datetime));
        const formattedDate = date.toLocaleString('en-GB', { timeZoneName: 'short' });
        return  formattedDate;
    }
    return <div>
        <p>HELOJ detta en detailed ticket for ticket id: {ticketId} </p>
        <button onClick={() => SetRefresh(current => !current)}>Refresh</button>
        <ul id={"ticketList"}>
        
            <li key={"ticketId-" + Ticket.ticketId}>
                <p className={"ticketId"}>{Ticket.ticketId}</p>
                <p className={"category"}>{Ticket.category}</p>
                <p className={"subcategory"}>{Ticket.subcategory}</p>
                <p className={"title"}>{Ticket.title}</p>
                <p className={"timeposted"}>{datetimeFormatter(Ticket.timePosted)}</p>
            </li>
    </ul>
    <ul id={"messageList"}>
        {Messages.map((Message=>
            <li key={"messageId-" + Message.messageId}>
                <p className={"messageId"}>{Message.messageId}</p>
                <p className={"title"}>{Message.title}</p>
                <p className={"message"}>{Message.message}</p>
                <p className={"messageSender"}>{Message.userId}</p>
            </li>
        ))}
    </ul>
        
    </div>
}

function MessageList({Messages}){

    
    return <ul id={"messageList"}>
        {Messages.map((Message=>
            <li key={Message.messageId}>
                <p className={"messageId"}>{Message.messageId}</p>
                <p className={"title"}>{Message.title}</p>
                <p className={"message"}>{Message.message}</p>
                <p className={"messageSender"}>{Message.userId}</p>
            </li>
        ))}
    </ul>
}