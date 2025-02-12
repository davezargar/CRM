import { useState, use, createContext } from 'react';
import {RoleContext} from "./main.jsx";
import NavBar from "./NavBar.jsx";
import "./style/CustomerServicePanel.css"
import TicketDisplayAll from "./TicketDisplayAll.jsx";

export default CustomerServicePanel;

function CustomerServicePanel()
{
    return <RoleContext.Provider value={"CustomerService"}>
        <div id="CustomerServicePanel">
            <NavBar/>
            <p>hi customer service :)</p>
            <div id={"CustomerServicePanelMain"}>
                <TicketDisplayAll/>
            </div>
            
        </div>
    </RoleContext.Provider>
}