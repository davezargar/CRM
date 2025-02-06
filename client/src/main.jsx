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
    function verifyLogin(){
        
    }
    
    return <form onSubmit={verifyLogin}>
        <label>email: <input type="text" name="email"/></label>
        <label>password: <input type="password" name="password"/></label>
        <input type="submit" value="Sign in"/>
    </form>
}

