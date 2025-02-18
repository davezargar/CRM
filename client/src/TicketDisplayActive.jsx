import { useState, useEffect } from "react";
import { NavLink } from "react-router";
import "./style/TicketDisplayActive.css";

export default TicketDisplayActive

function TicketDisplayActive()
{
    const [Refresh, SetRefresh] = useState(false);
    
    return <div id={"TicketDisplayActive"}>
        <button onClick={() => SetRefresh(current => !current)}>Refresh</button>
        <p>ticket display active</p>
        <TicketList refresh={Refresh}/>
    </div>
}

function TicketList({refresh}) {
    const [Tickets, SetTickets] = useState([]);

    useEffect(()=>{
        fetch("/api/ticketList")
            .then(response => response.json())
            .then(data=> {
                console.log(data);
                SetTickets(data);
            });
    }, [refresh]);
    
    function datetimeFormatter(datetime){
        const date = new Date(Date.parse(datetime));
        const formattedDate = date.toLocaleString('en-GB', { timeZoneName: 'short' });
        return  formattedDate;
    }
    
    return <ul id={"ticketList"}>
        {Tickets.map((ticket)=><NavLink to={`/CustomerServicePanel/ticket/${ticket.ticketId}`}>
            <li key={"ticketId-" + ticket.ticketId}>
                <p className={"ticketId"}>{ticket.ticketId}</p>
                <p className={"category"}>{ticket.category}</p>
                <p className={"subcategory"}>{ticket.subcategory}</p>
                <p className={"title"}>{ticket.title}</p>
                <p className={"timeposted"}>{datetimeFormatter(ticket.timePosted)}</p>
            </li>
        </NavLink>)}
    </ul>
}