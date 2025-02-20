import { NavLink, useNavigate } from "react-router";
import NavBar from "./NavBar";
import "./style/removeAddCustomerSupport.css";
import { useEffect, useState } from "react";
import "./style/adminPanel.css";




export function AddRemoveCustomerSupport() {
    const navigate = useNavigate();

    return (
        <div className="sec">
            <div className="buttonContainer">
                <button onClick={() => navigate('/addCustomer')}>Add Customer Support</button>
                <button onClick={() => navigate('/removeCustomer')}>Remove Customer Support</button>
            </div>
            <NavBar />
        </div>
    )

}

export function AddCustomer() {
    const [email, setEmail] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("/api/addCustomer", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email }),
            });
            if (!response.ok) {
                throw new Error("Something went wrong with adding worker");
            }
            alert("Worker added!");
        } catch (error) {
            console.error(error);
            alert("Couldn't add custom service worker");
        }
    }

    return (
        <form onSubmit={handleSubmit}>
            <NavLink to="/addCustomer"><label>Email: <input type="email" name="email" required value={email}
                onChange={(e) => setEmail(e.target.value)} /></label></NavLink>
            <button type="submit">Add Customer Support Worker</button>
            <NavBar />
        </form>
    )
}


export function RemoveCustomer() {
    const [email, setEmail] = useState("");
    const [emails, setEmails] = useState([]);

    useEffect(() => {
        async function fetchEmails() {
            try {
                const response2 = await fetch("/api/getCustomerSupport", {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                    },
                }); if (!response2.ok) {
                    throw new Error("Something went wrong getting the workers")
                }
                const data = await response2.json();
                console.log(data)
                setEmails(data)

            } catch (error) {
                console.error(error);
                alert("Couldn't find custom service worker");
            }
        }
        fetchEmails();
    }, []);


    const handleSubmit = async (emailToRemove) => {

        try {
            const response = await fetch("/api/removeCustomer", {
                method: "DELETE",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ email: emailToRemove }),
            });
            if (!response.ok) {
                throw new Error("Something went wrong with adding worker");
            }
            alert("Customer Support Worker Removed!");
            setEmails((previousMail) => previousMail.filter((item) => item.email !== emailToRemove));
        } catch (error) {
            console.error(error);
            alert("Couldn't add custom service worker");
        }
    }

    return (
        <div className="removeContainer">
            <NavBar></NavBar>
            <h1>Custom Support Workers</h1>
            <ul className="listOfEmails">
                {emails.map((emailItem) => (
                    <li key={emailItem.email}>
                        {emailItem.email}
                        <button className="removeButtonAdmin" onClick={() => handleSubmit(emailItem.email)}>Remove</button>
                    </li>
                ))}
            </ul>
        </div>

        /*
        <form onSubmit={handleSubmit}>
            <NavLink to="/removeCustomer"><label>Email: <input type="text" name="email" required value={email}
                onChange={(e) => setEmail(e.target.value)} /></label></NavLink>
            <button type="submit">Remove Customer Support Worker</button>
            <NavBar />
        </form> */
    )
}

