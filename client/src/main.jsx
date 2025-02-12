import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink, useNavigate} from "react-router";
import DefaultPage from "./DefaultPage";
import ActiveTickets from './ActiveTickets';
import customePanel from "./customerPanel"
createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)

function App()
{
    return <BrowserRouter>
        <Routes>
            <Route index element={<LoginForm/>}></Route>
            <Route path={"/customerPanel"} element={<customerPanel/>}/>
            <Route path="/DefaultPage" element={<DefaultPage/>}/>
            <Route path="/ActiveTickets" element={<ActiveTickets/>}/>
            <Route path="/register" element={<RegisterForm />} />
        </Routes>
    </BrowserRouter>
}

function LoginForm()
{
    function verifyLogin(e){
        e.preventDefault();
        const form = e.target;
        const formData = new FormData(form)
        const loginData = Object.fromEntries(formData.entries());
        
        const navigate = useNavigate();
        
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
            if(data === "customer")
            {
                console.log("hi");
                navigate(customerPanel);
            }
        })
    }
    
    function test(e){
        e.preventDefault();
        fetch("/api/test")
            .then(response=>response.json())
            .then(data=>alert(data));
    }
    
    return <form onSubmit={verifyLogin}>
        <label>email: <input type="text" name="email"/></label>
        <label>password: <input type="password" name="password"/></label>
        <NavLink to="/DefaultPage"><input type="submit" value="Sign in"/></NavLink>
        <input type="submit" value="Sign in"/>
        <NavLink to="/register">
            <button type="button">Register</button>
        </NavLink>
        <button onClick={test}>test auth</button>
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