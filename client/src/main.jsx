import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink} from "react-router";
import DefaultPage from "./DefaultPage";
import ActiveTickets from './ActiveTickets';
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
            <Route path="/DefaultPage" element={<DefaultPage/>}/>
<<<<<<< HEAD
            <Route path="/ActiveTickets" element={<ActiveTickets/>}/>
=======
            <Route path="/register" element={<RegisterForm />} />
>>>>>>> 0722946e5f164347d9b98e618263d0a597fcce8d
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
import ActiveTickets from './ActiveTickets';
            <Route path="/register" element={<RegisterForm />} />
            <Route path="/ActiveTickets" element={<ActiveTickets/>}/>
        <input type="submit" value="Sign in"/>
        <NavLink to="/register">
            <button type="button">Register</button>
        </NavLink>
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

