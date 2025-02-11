import { NavLink, useNavigate } from "react-router";
import NavBar from "./NavBar";
import "./style/removeAddCustomerSupport.css";
import { useEffect, useState } from "react";




export function AddRemoveCustomerSupport() {
    const navigate = useNavigate();
    const [redirect, setRedirect] = useState(false);

    useEffect(() => {
        if (redirect) {
            navigate('/addCustomer');
        }
    }, [redirect, navigate])

    return (
        <div className="sec">
            <div className="buttonContainer">
                <button onClick={() => setRedirect(true)}>Add Customer Support</button>
                <button onClick={() => navigate('/removeCustomer')}>Remove Customer Support</button>
            </div>

        </div>
    )

}

export function AddCustomer() {
    console.log("hej")
    return (
        <form>
            <NavLink to="/addCustomer"><label>Email: <input type="text" name="email" /></label></NavLink>
        </form>
    )
}

export function RemoveCustomer() {
    return (
        <div>
            <p>hej</p>
        </div>
    )
}

