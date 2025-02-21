import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink, useNavigate } from "react-router";

import DefaultPage from "./DefaultPage";
import ActiveTickets from './ActiveTickets';

import CustomerServicePanel from "./CustomerServicePanel";
import TicketDisplayActive from "./TicketDisplayActive.jsx";
import TicketDetailed from "./TicketDetailed.jsx";

import { AddRemoveCustomerSupport } from './adminPanel';
import { AddCustomer, RemoveCustomer } from './adminPanel';

import CustomerPanel from "./CustomerPanel";
import CreateTicket from "./CreateTicket";

import "./style/Login.css";

export const RoleContext = createContext({});

createRoot(document.getElementById('root')).render(
    <StrictMode>
        <App />
    </StrictMode>
)

function App() {
    return <RoleContext.Provider value={"Customer"}>
        <BrowserRouter>
            <Routes>
                <Route index element={<Index />} />
                <Route path={"/CustomerPanel"} element={<CustomerPanel />} />

                <Route path={"/CustomerServicePanel"} element={<CustomerServicePanel />}>
                    <Route path={"/CustomerServicePanel/tickets"} element={<TicketDisplayActive />} /> {/*Should default to this path dont know how*/}
                    <Route path={"/CustomerServicePanel/ticket/:ticketId"} element={<TicketDetailed />} />
                    {/*Route account settings*/}
                </Route>

                <Route path='/adminPanel' element={<AddRemoveCustomerSupport />} />
                <Route path='/addCustomer' element={<AddCustomer />} />
                <Route path='/removeCustomer' element={<RemoveCustomer />} />

                <Route path="/CreateTicket" element={<CreateTicket />} />
                <Route path="/DefaultPage" element={<DefaultPage />} />
                <Route path="/ActiveTickets" element={<ActiveTickets />} />
                <Route path="/register" element={<RegisterForm />} />
            </Routes>
        </BrowserRouter>
    </RoleContext.Provider>
}

function Index() {
    return <div>
        <LoginForm />
        <QuickNav />
    </div>
}

function QuickNav() {
    function test() {
        fetch("/api/test")
            .then(response => response.json())
            .then(data => alert(data));
    }
    /*
        return <div id={"QuickNav"}>
            <NavLink to="/DefaultPage"><button>Defualt page</button></NavLink>
            <NavLink to="/CustomerPanel"><button>CustomerPanel</button></NavLink>
            <NavLink to="/AdminPanel"><button>AdminPanel</button></NavLink>
            <NavLink to="/CustomerServicePanel/tickets"><button>CustomerServicePanel</button></NavLink>
            <NavLink to="/CreateTicket"><button type="button">Create Ticket</button></NavLink>
            <button onClick={test}>test auth</button>
        </div> */
}

function LoginForm() {
    const navigate = useNavigate();

    function verifyLogin(e) {
        e.preventDefault();
        const form = e.target;
        const formData = new FormData(form)
        const loginData = Object.fromEntries(formData.entries());

        fetch("/api/login", {
            headers: { "Content-Type": "application/json" },
            method: "POST",
            body: JSON.stringify(loginData)
        })
            .then(response => {
                if (response.ok) {
                    console.log("response.ok")
                    console.log(response);
                    return response.json();
                }
            })
            .then(data => {
                console.log(data);
                switch (data) {
                    case "customer":
                        navigate("/customerPanel");
                        break;
                    case "admin":
                        navigate("/AdminPanel");
                        break;
                    case "customerService":
                        navigate("/CustomerServicePanel/tickets");
                        break;
                }
            })
    }


    return <form className='formContainer' onSubmit={verifyLogin}>
        <div className='registerCon'>
            <p>Don't have an account?</p>
            <NavLink to="/register">
                <button type="button">Register here!</button>
            </NavLink></div>
        <div className="inputCon">
            <label>Email: </label>
            <input type="text" name="email" />
            <label>Password: </label>
            <input type="password" name="password" />
            <input className='submitButton' type="submit" value="Sign in" />
        </div>
    </form>
}
function RegisterForm() {
    function handleRegister() {

    }

    return (
        <form className='formContainer' onSubmit={handleRegister}>
            <div className='registerCon'>
                <p>Already have an account?</p>
                <NavLink to="/LoginForm">
                    <button type="button">Login here!</button>
                </NavLink></div>
            <div className='inputCon'>
                <label>email: </label>
                <input type="text" name="email" />
                <label>password: </label>
                <input type="password" name="password" />
                <input className='submitButton' type="submit" value="Register" />
            </div>
        </form >
    );
}

