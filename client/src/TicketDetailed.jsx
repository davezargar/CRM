import { Outlet, useParams } from "react-router";
import { useState, useEffect } from "react";
import MessageBox from './MessageBox';


export default TicketDetailed;

function TicketDetailed() {
    const [Ticket, SetTicket] = useState([]);
    let params = useParams();
    let ticketId = params.ticketId;
    let token = params.token;
    const [Refresh, SetRefresh] = useState(false);

    const [Messages, SetMessages] = useState([]);
    
    useEffect(() => {
        if(token == null || token === "")
        {
            fetch(`/api/tickets/${ticketId}`)
                .then(response => response.json())
                .then(data => {
                    console.log(data);
                    SetMessages(data.messages);
                    SetTicket(data.ticketRecord);
                });
        }else
        {
            fetch(`/api/customer/tickets/${token}`)
                .then(response => response.json())
                .then(data => {
                    console.log(data);
                    SetMessages(data.messages);
                    SetTicket(data.ticketRecord);
                });
        }
        
    }, [Refresh]);

    function datetimeFormatter(datetime) {
        const date = new Date(Date.parse(datetime));
        const formattedDate = date.toLocaleString('en-GB', { timeZoneName: 'short' });
        return formattedDate;
    }

    function checkStatus(Ticket) {
        if (Ticket.timeClosed == null) {
            return <p>Status: Ongoing</p>
        } else {
            return <p>Status: Resolved</p>
        }
    }

    return <div id={"ticket-detailed"}>
        <span id={"ticketflex"}>
        <div className="info">
            <ul id={"information"}>
                <li key={Ticket.ticketId}>
                    <p>From: {Ticket.userEmail} </p> {checkStatus(Ticket)}
                    <p>Created: {datetimeFormatter(Ticket.timePosted)} </p>
                </li>
            </ul>
            
        </div>
        <div className="refreshContainer">
            <button className="refreshButton" onClick={() => SetRefresh(current => !current)}>Refresh</button>
        </div>
            </span>
        <ul id={"ticketList"}>
            <li key={Ticket.ticketId}>
                <p className={"category"}>Main category: {Ticket.category}</p>
                <p className={"subcategory"}>Subcategory: {Ticket.subcategory}</p>
                <p className={"title"}>Title: {Ticket.title}</p>
            </li>
        </ul>
        <div className="messageContainer">
            <ul id={"messageList"}>
                {Messages.map((Message =>
                    <li key={Message.messageId}>
                        <p className={"time_Posted"}>Time sent: {datetimeFormatter(Message.timePosted)}</p>
                        <h3><p className={"title"}>{Message.title}</p></h3>
                        <h4>Message:</h4>
                        <p className={"message"}>{Message.message}</p>
                        <h5>From:<p className={"messageSender"}>{Message.userId}</p></h5>
                    </li>
                ))}
            </ul>
        </div>
        <MessageBox ticket_Id={ticketId}></MessageBox>
    </div>
}

function MessageList({ Messages }) {

    
    return <ul id={"messageList"}>
        {Messages.map((Message =>
            <li key={Message.messageId}>
                <p className={"time_Posted"}>{Message.timePosted} </p>
                <p className={"messageId"}>{Message.messageId}</p>
                <p className={"title"}>{Message.title}</p>
                <p className={"message"}>{Message.message}</p>
                <p className={"messageSender"}>{Message.userId}</p>
            </li>
        ))}
    </ul>
}