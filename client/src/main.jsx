import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink, useNavigate} from "react-router";
import DefaultPage from "./DefaultPage";
import ActiveTickets from './ActiveTickets';
import { AddRemoveCustomerSupport } from './adminPanel';
import { AddCustomer, RemoveCustomer } from './adminPanel';
import CustomerServicePanel from "./CustomerServicePanel";
import CustomerPanel from "./CustomerPanel";
import CreateTicket from "./CreateTicket";
createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App/>
  </StrictMode>
)

function App()
{
    return <BrowserRouter>
        <Routes>
            <Route index element={<div><LoginForm/><QuickNav/></div>}></Route>
            <Route path={"/CustomerPanel"} element={<CustomerPanel/>}/>
            <Route path={"/CustomerServicePanel"} element={<CustomerServicePanel/>}/>
            <Route path="/DefaultPage" element={<DefaultPage/>}/>
            <Route path="/ActiveTickets" element={<ActiveTickets/>}/>
            <Route path='/addCustomer' element={<AddCustomer />} />
            <Route path='/removeCustomer' element={<RemoveCustomer />}/>
            <Route path="/register" element={<RegisterForm />} />
            <Route path='/adminPanel' element={<AddRemoveCustomerSupport />} />

            <Route path="/CreateTicket" element={<CreateTicket/>}/>
        </Routes>
    </BrowserRouter>
}

function QuickNav()
{
    function test(){
        fetch("/api/test")
            .then(response=>response.json())
            .then(data=>alert(data));
    }
    
    return <div id={"QuickNav"}>
        <NavLink to="/DefaultPage"><button>Defualt page</button></NavLink>
        <button onClick={test}>test auth</button>
    </div>
}

function LoginForm() {
    const navigate = useNavigate();
    
    function verifyLogin(e){
        e.preventDefault();
        const form = e.target;
        const formData = new FormData(form)
        const loginData = Object.fromEntries(formData.entries());

        fetch("/api/login", {
            headers: { "Content-Type": "application/json" },
            method: "POST",
            body: JSON.stringify(loginData)
        })
        .then(response=>{
            if(response.ok)
            {
                console.log("response.ok")
                console.log(response);
                return response.json();
            }
        })
        .then(data =>{
            console.log(data);
            switch(data)
            {
                case "customer":
                    navigate("/customerPanel");
                    break;
                case "admin":
                    navigate("/AdminPanel");
                    break;
                case "customerService":
                    navigate("/CustomerServicePanel");
                    break;
            }
        })
    }
    
    
    return <form onSubmit={verifyLogin}>
        <label>email: <input type="text" name="email"/></label>
        <label>password: <input type="password" name="password"/></label>
        <input type="submit" value="Sign in"/>
        <NavLink to="/register">
            <button type="button">Register</button>
        </NavLink>
        
        <NavLink to="/CreateTicket">
        <button type="button">Create Ticket</button></NavLink>
    </form>
}
function RegisterForm() {
    function handleRegister() {
        
    }

    return (
        <form onSubmit={handleRegister}>
            <label>email: <input type="text" name="email" /></label>
            <label>password: <input type="password" name="password" /></label>
            <input type="submit" value="Register" />
        </form>
    );
}

