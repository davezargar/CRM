import { NavLink, useNavigate, Outlet } from "react-router";
import NavBar from "./NavBar";
import "./style/removeAddCustomerSupport.css";
import { useEffect, useState } from "react";
import "./style/adminPanel.css";
import { RoleContext } from "./main.jsx";



export function AdminPanel() {
    return <div id="admin-panel">
        <RoleContext.Provider value={"Admin"}>
            <NavBar />

            <div id={"admin-panel-main"}>
                <Outlet /> {/*child routes will be rendered here??*/}
            </div>
        </RoleContext.Provider>
    </div>
}


export function AddRemoveWorkers() {
    const navigate = useNavigate();

    return (
        <div className="sec">
            <div className="buttonContainer">
                <button onClick={() => navigate('./add-worker')}>Add Customer Support</button>
                <button onClick={() => navigate('./remove-worker')}>Remove Customer Support</button>
            </div>
        </div>
    )

}

export function AddWorker() {
    const [email, setEmail] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("/api/workers/password", {
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
        <form className="form-Container" onSubmit={handleSubmit}>
            <div className="form-Item">
                <label>Email: </label>
                <input type="email" name="email" required value={email}
                    onChange={(e) => setEmail(e.target.value)} />
                <button type="submit">Add Customer Support Worker</button>
            </div>
        </form>
    )
}


export function RemoveWorker() {
    const [email, setEmail] = useState("");
    const [emails, setEmails] = useState([]);

    useEffect(() => {
        async function fetchEmails() {
            try {
                const response2 = await fetch("/api/workers", {
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
            const response = await fetch("/api/workers", {
                method: "put",
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

