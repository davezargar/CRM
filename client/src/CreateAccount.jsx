import { useState} from "react"
import "./style/Login.css"

function CreateAccount() {
    const [formData, setFormData] = useState({
        email: "",
        password: "",
    });

    const handleChange = (e) => {
        setFormData({
            ...formData, [e.target.name]: e.target.value,
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("/api/CreateAccount", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(formData),
            });

            if (!response.ok) {
                throw new Error("Something went wrong when creating account");
            }

            alert("Account created successfully");
        } catch (error) {
            console.error(error);
            alert("Couldn't create account.");
        }
    };

    return (
        <div id="createAccount">
            <h2>Register</h2>
            
            <form id="accountForm" onSubmit={handleSubmit}>
                <h3>Email</h3>
                <input type="email" id="Email" value={formData.Email} onChange={handleChange}/>

                <h3>Password</h3>
                <input type="password" id="Password" value={formData.Password} onChange={handleChange}/>

                <br/>
                <button type="submit">Create Account</button>
            </form>
        </div>
    );
}
    export default CreateAccount;