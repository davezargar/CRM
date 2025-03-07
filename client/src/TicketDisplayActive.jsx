import { useState, useEffect, useContext, createContext } from "react";
import { NavLink, useNavigate } from "react-router";
import "./style/TicketDisplayActive.css";

export default TicketDisplayActive

function TicketDisplayActive()
{
    const [Refresh, SetRefresh] = useState(false);
    const [Tickets, SetTickets] = useState([]); //https://react.dev/learn/sharing-state-between-components
    
    return <div id={"TicketDisplayActive"}>
        <header>
            <div>
                <h3>active tickets: {Tickets.length}</h3>
                <button onClick={() => SetRefresh(current => !current)}>Refresh</button>
            </div>
        </header>
        <TicketTable refresh={Refresh} tickets={Tickets} setTickets={(newData) => SetTickets(newData)}/>
    </div>
}

function TicketTable({refresh, tickets, setTickets}) {
    const navigate = useNavigate();
    const [sortId, setSortId] = useState("desc");

    useEffect(()=>{
        fetch("/api/tickets")
            .then(response => response.json())
            .then(data=> {
                console.log(data);
                setTickets(data);
            });
    }, [refresh]);
    
    function datetimeFormatter(datetime){
        const date = new Date(Date.parse(datetime));
        const formattedDate = date.toLocaleString('en-GB', { timeZoneName: 'short' });
        return  formattedDate;
    }
    
    function navigateToTicket(e)
    {
        let id = e.currentTarget.children[0].innerText;
        navigate("./" + id);
    }

    function sortById()
    {
        const sortedTickets = [...tickets].sort((a, b) =>
        sortId === "desc" ? a.ticketId - b.ticketId : b.ticketId - a.ticketId
    );
        setTickets(sortedTickets);
        setSortId(sortId === "desc" ? "asc" : "desc");
    
    }
    
    return <div id={"ticketTableContainer"}>
        <table>
            <thead>
                <tr>
                    <th>
                        <button className={"ticket-id"} onClick={sortById}>{sortId === "desc" ? "ID↑" : "ID↓"}</button>
                    </th>
                    <th className={"category"}>Category</th>
                    <th className={"subcategory"}>Subcategory</th>
                    <th className={"title"}>Title</th>
                    <th className={"time-posted"}>Time Posted</th>
                    <th className={"ticket-status"}>Status</th>
                </tr>
            </thead>
            <tbody>
                {tickets.map((ticket)=><tr key={ticket.ticketId} onClick={navigateToTicket}>
                    <th className={"ticketId"}>{ticket.ticketId}</th>
                    <td className={"category"}>{ticket.category}</td>
                    <td className={"subcategory"}>{ticket.subcategory}</td>
                    <td className={"title"}>{ticket.title}</td>
                    <td className={"time-posted"}>{datetimeFormatter(ticket.timePosted)}</td>
                    <td className={"ticket-status"}>Active</td>
                </tr>)}
            </tbody>
        </table>
    </div>
}