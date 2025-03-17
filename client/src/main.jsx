import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink, useNavigate } from "react-router";

import CustomerServicePanel from "./CustomerServicePanel";
import TicketDisplayActive from "./TicketDisplayActive.jsx";
import TicketDetailed from "./TicketDetailed.jsx";
import TicketDisplayResolved from "./TicketDisplayResolved.jsx";

import { AddRemoveWorkers, AdminPanel } from './adminPanel';
import { AddWorker, RemoveWorker } from './adminPanel';
import { AssignTickets } from "./AssignTickets.jsx";

import FeedbackForm from "./FeedbackForm.jsx";
import CustomerPanel from "./CustomerPanel";
import { CreateTicket, CreateIkeaTicket, CreateMikromjukTicket } from "./CreateTicket";

import "./style/Login.css";
import CreateAccount from "./CreateAccount.jsx";
import ChangePassword from './ChangePassword.jsx';

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
                <Route path={"/customer-panel"} element={<CustomerPanel />} />
                <Route path={"/customer-service-panel"} element={<CustomerServicePanel />}>
                    <Route path={"/customer-service-panel/tickets"} element={<TicketDisplayActive />} /> {/*Should default to this path dont know how*/}
                    <Route path={"/customer-service-panel/tickets/:ticketId"} element={<TicketDetailed />} />
                    <Route path={"/customer-service-panel/tickets/resolved"} element={<TicketDisplayResolved />} />
                    {/*Route account settings*/}
                </Route>

                <Route path='/admin-panel' element={<AdminPanel />} >
                    <Route index element={<AddRemoveWorkers />} />
                    <Route path='add-worker' element={<AddWorker />} />
                    <Route path='remove-worker' element={<RemoveWorker />} />
                    <Route path='assign-tickets' element={<AssignTickets />} />
                    <Route path='change-password' element={<ChangePassword />} />
                </Route>

                <Route path={"/tickets/:ticketId/:token?"} element={<TicketDetailed />} />
                <Route path={"/feedback-form/:token?"} element={<FeedbackForm />} />
                <Route path="/create-ticket">
                    <Route path={"ikea-form"} element={<CreateIkeaTicket />} />
                    <Route path={"mikromjuk-form"} element={<CreateMikromjukTicket />} />
                </Route>
                <Route path="/register" element={<RegisterForm />} />
                <Route path="/customers" element={<CreateAccount />} />
            </Routes>
        </BrowserRouter>
    </RoleContext.Provider>
}

function Index() {
    return <div>
        <LoginForm />
        <TicketForms />
    </div>
}

function TicketForms() {

    return <div id={"ticket-forms-nav"}>
        <NavLink to={"/create-ticket/ikea-form"}><button type="button">ikea form</button></NavLink>
        <NavLink to={"/create-ticket/mikromjuk-form"}><button type="button">mikromjuk form</button></NavLink>
    </div>
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
                        navigate("/customer-panel");
                        break;
                    case "admin":
                        navigate("/admin-panel");
                        break;
                    case "support":
                        navigate("/customer-service-panel/tickets");
                        break;
                }
            })
    }


    return <form className='formContainer' onSubmit={verifyLogin}>
        <div className='registerCon'>
        </div>
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
                <NavLink to="/">
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

