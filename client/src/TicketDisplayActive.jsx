import { useState, useEffect } from "react";
import { NavLink } from "react-router";
import "./style/TicketDisplayActive.css";

export default TicketDisplayActive

function TicketDisplayActive()
{
    const [tickets, setTickets] = useState([]);
    
    useEffect(fetchTickets);
    return <div id={"TicketDisplayActive"}>
        <button onClick={fetchTickets}>Refresh</button>
        <p>ticket display active</p>

        <ul id={"ticketList"}>
            {tickets.map(Ticket)}
        </ul>
    </div>

    function fetchTickets() {
        fetch("/api/ticketList")
            .then(response => response.json())
            .then(data => {
                console.log(data);
                SetTickets(data);
            });
    }
}

function Ticket(ticket) {


    function datetimeFormatter(datetime) {
        const date = new Date(Date.parse(datetime));
        const formattedDate = date.toLocaleString('en-GB', {timeZoneName: 'short'});
        return formattedDate;
    }

    return <NavLink to={`/CustomerServicePanel/ticket/${ticket.ticketId}`}>
            <li key={ticket.ticketId}>
                <p className={"ticketId"}>{ticket.ticketId}</p>
                <p className={"category"}>{ticket.category}</p>
                <p className={"subcategory"}>{ticket.subcategory}</p>
                <p className={"title"}>{ticket.title}</p>
                <p className={"timeposted"}>{datetimeFormatter(ticket.timePosted)}</p>
            </li>
    </NavLink>
}