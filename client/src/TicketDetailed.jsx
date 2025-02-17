import { Outlet, useParams } from "react-router";


export default TicketDetailed;

function TicketDetailed(){
    let params = useParams();
    
    let ticketId = params.ticketId;
    
    return <div>
        <p>HELOJ detta en detailed ticket for ticket id: {ticketId} </p>
    </div>
}