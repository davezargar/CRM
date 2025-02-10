import { useState } from 'react'
import { NavLink } from "react-router";
import "./style/NavBar.css"
const NavLinks = {
    Admin:[
        {name: "Active Tickets", path: "/ActiveTickets"},
        {name: "Edit/Redirect Ticket", path: "/Edit/RedirectTicket"},
        {name: "Resolved tickets", path: "/ResolvedTickets"},
        {name: "Add/Remove Customer", path: "/Add/RemoveCustomer"},
        {name: "Customer supp direct", path: "/CustomerSuppDirect"}
    ],
    Customer:[
        {name: "Create Ticket", path: "/CreateTicket"},
        {name: "Your Ticket History", path: "/YourTicketHistory"}
    ],
    CustomerService: [
        {name: "Active Tickets", path: "/ActiveTickets"},
        {name: "Edit/Redirect Ticket", path: "/Edit/RedirectTicket"},
        {name: "Resolved tickets", path: "/ResolvedTickets"},
    ]
};

function NavBar()
{
    const [role, setRole] = useState("Admin");
    
    return (<nav>
        <ul><h1 id='Menu'>Menu</h1>
            {NavLinks[role].map((link, index) =>(
                <li key={index}>
                    <NavLink className="Options" to={link.path}>{link.name}</NavLink>
                </li>
            ))}
        </ul>
    </nav>)
}

export default NavBar;