import { StrictMode, useState, createContext, use, useEffect } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, NavLink} from "react-router";


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
        <input type="submit" value="Sign in"/>
        <button onClick={test}>test auth</button>
    </form>
}

