import { useState, use } from 'react'
import { NavLink } from "react-router";
import "./style/NavBar.css"
import { RoleContext } from "./main.jsx";
const NavLinks = {
    Admin: [
        { name: "Manage workers", path: "./" },
        { name: "Add worker", path: "./add-worker" },
        { name: "Remove worker", path: "/remove-worker" }
    ],
    Customer: [
        { name: "Create Ticket", path: "/customer-panel/create-ticket" },
        { name: "Your Ticket History", path: "/customer-panel/ticket-history" },
        { name: "account settings", path: "/customer-panel/account-settings" }
    ],
    CustomerService: [
        { name: "Active Tickets", path: "/customer-service-panel/tickets" },
        { name: "Resolved tickets", path: "/customer-service-panel/resolved-tickets" },
        { name: "Account settings", path: "/customer-service-panel/account-settings" },
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
                            <li key={("navkey- " + index)}>
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