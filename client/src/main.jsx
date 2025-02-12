import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink, useNavigate} from "react-router";

import TicketDisplayActive from "./TicketDisplayActive.jsx";
import DefaultPage from "./DefaultPage";
import ActiveTickets from './ActiveTickets';
import AdminPanel from "./AdminPanel";
import CustomerServicePanel from "./CustomerServicePanel";
import CustomerPanel from "./CustomerPanel";

export const RoleContext = createContext({});

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App/>
  </StrictMode>
)

function App()
{
    return <RoleContext.Provider value={"Customer"}>
        <BrowserRouter>
            <Routes>
                <Route index element={<Index/>}/>
                <Route path={"/CustomerPanel"} element={<CustomerPanel/>}/>
                <Route path={"/CustomerServicePanel"} element={<CustomerServicePanel/>}>
                    <Route index element={<TicketDisplayActive/>}/>  
                </Route>
                <Route path={"/AdminPanel"} element={<AdminPanel/>}/>
                
                
                <Route path="/DefaultPage" element={<DefaultPage/>}/>
                <Route path="/ActiveTickets" element={<ActiveTickets/>}/>
                <Route path="/register" element={<RegisterForm />} />
            </Routes>
        </BrowserRouter>
    </RoleContext.Provider>
}

function Index(){
    return <div>
        <LoginForm/>
        <QuickNav/>
    </div>
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
        <NavLink to="/CustomerPanel"><button>CustomerPanel</button></NavLink>
        <NavLink to="/AdminPanel"><button>AdminPanel</button></NavLink>
        <NavLink to="/CustomerServicePanel"><button>CustomerServicePanel</button></NavLink>
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