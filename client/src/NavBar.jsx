import { useState, use } from 'react'
import { NavLink } from "react-router";
import "./style/NavBar.css"
import { RoleContext } from "./main.jsx";
const NavLinks = {
    Admin: [
        { name: "Manage workers", path: "./" },
        { name: "Add worker", path: "./addCustomer" },
        { name: "Remove worker", path: "/removeCustomer" }
    ],
    Customer: [
        { name: "Create Ticket", path: "/CustomerPanel/CreateTicket" },
        { name: "Your Ticket History", path: "/CustomerPanel/TicketHistory" },
        { name: "account settings", path: "/CustomerPanel/accountSettings" }
    ],
    CustomerService: [
        { name: "Active Tickets", path: "/CustomerServicePanel/tickets" },
        { name: "Resolved tickets", path: "/CustomerServicePanel/ResolvedTickets" },
        { name: "Account settings", path: "/CustomerServicePanel/accountSettings" },
    ]
};

function NavBar() {
    const role = use(RoleContext);


    return (<div id={"nav-container"}>
            <nav>
                <ul><h1 id='Menu'>Account Email:</h1>
                    <p className='roleText'>{role}</p>
                    {NavLinks[role].map((link, index) => (
                        <div className='optionContainer'>
                            <li key={index}>
                                <NavLink className="Options" to={link.path}>{link.name}</NavLink>
                            </li>
                        </div>
                    ))}
                </ul>
            </nav>
        </div>
    )
}

export default NavBar;