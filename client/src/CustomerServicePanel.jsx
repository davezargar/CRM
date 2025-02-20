import { useState, use, createContext } from 'react';
import { RoleContext } from "./main.jsx";
import { Outlet } from "react-router";


import NavBar from "./NavBar.jsx"
import "./style/CustomerServicePanel.css";

export default CustomerServicePanel;

function CustomerServicePanel() {
    return <div id="CustomerServicePanel">
            <RoleContext.Provider value={"CustomerService"}>
                <NavBar/>
                <div id={"CustomerServicePanelMain"}>
                    <Outlet/> {/*child routes will be rendered here??*/}
                </div>
            </RoleContext.Provider>
    </div>

}