import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink, useNavigate } from "react-router";

import CustomerServicePanel from "./CustomerServicePanel";
import TicketDisplayActive from "./TicketDisplayActive.jsx";
import TicketDetailed from "./TicketDetailed.jsx";

import { AddRemoveWorkers, AdminPanel } from './adminPanel';
import { AddWorker, RemoveWorker } from './adminPanel';

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
                <Route path={"/customer-panel"} element={<CustomerPanel />} />

                <Route path={"/customer-service-panel"} element={<CustomerServicePanel />}>
                    <Route path={"/customer-service-panel/tickets"} element={<TicketDisplayActive />} /> {/*Should default to this path dont know how*/}
                    <Route path={"/customer-service-panel/tickets/:ticketId"} element={<TicketDetailed />} />
                    {/*Route account settings*/}
                </Route>

                <Route path='/admin-panel' element={<AdminPanel/>} >
                    <Route index element={<AddRemoveWorkers/>}/>
                    <Route path='add-worker' element={<AddWorker/>}/>
                    <Route path='remove-worker' element={<RemoveWorker/>} />
                </Route>
                

                <Route path="/create-ticket" element={<CreateTicket />} />
                <Route path="/register" element={<RegisterForm />} />
            </Routes>
        </BrowserRouter>
    </RoleContext.Provider>
}

function Index() {
    return <div>
        <LoginForm />
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
                    case "customerService":
                        navigate("/customer-service-panel/tickets");
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

