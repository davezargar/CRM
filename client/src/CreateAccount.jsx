import { useState } from "react";
import "./style/Login.css";

function CreateAccount() {
    const [formData, setFormData] = useState({
        email: "",
        password: "",
        company_fk: 1, // Example company ID
    });

    const handleChange = (e) => {
        setFormData({
            ...formData,
            [e.target.name]: e.target.value,
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("/api/customers", {
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
                <input type="email" name="email" value={formData.email} onChange={handleChange} />

                <h3>Password</h3>
                <input type="password" name="password" value={formData.password} onChange={handleChange} />

                <br/>
                <button type="submit">Create Account</button>
            </form>
        </div>
    );
}

export default CreateAccount