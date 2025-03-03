import { NavLink } from "react-router";
import { useState } from "react";
import "./style/CreateTicket.css";
import * as React from "react";

function CreateTicket() {
    const [formData, setFormData] = useState({
        CategoryName: "",
        SubcategoryName: "",
        Title: "",
        Message: "",
        UserEmail: "",
        CompanyFk: 4,
    });

    const tech = ["account", "software"];
    const billingSupport = ["Payment issue", "Insurance issue"];

    const handleChange = (e) => {
        const { id, value } = e.target;
        setFormData((prevData) => ({
            ...prevData,
            [id]: value,
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            console.log(formData);
            const response = await fetch("/api/tickets", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(formData),
            });
            if (!response.ok) {
                throw new Error("Something went wrong when creating ticket");
            }
            alert("request sent");
        } catch (error) {
            console.error(error);
            alert("Couldn't create ticket.");
        }
    };

    let subcategories = [];
    if (formData.CategoryName === "tech") {
        subcategories = tech;
    } else if (formData.CategoryName === "Billing Support") {
        subcategories = billingSupport;
    }

    return (
        <div id="createTicket">
            <h1>Create Ticket</h1>
            <form id="formId" onSubmit={handleSubmit}>
                <h3>Category (temp data)</h3>
                <select required id="CategoryName" value={formData.CategoryName} onChange={handleChange}>
                    <option value="">Choose...</option>
                    <option value="tech">tech</option>
                    <option value="Billing Support">Billing Support</option>
                </select>
                <br />
                <h3>Subcategory (temp data)</h3>
                <select required id="SubcategoryName" value={formData.SubcategoryName} onChange={handleChange}>
                    <option value="">Choose...</option>
                    {subcategories.map((sub) => (
                        <option key={sub} value={sub}>
                            {sub}
                        </option>
                    ))}
                </select>
                <h3>Title</h3>
                <input type="text" id="Title" value={formData.Title} onChange={handleChange} />
                <h3>Message</h3>
                <textarea
                    id="Message"
                    rows={12}
                    cols={100}
                    value={formData.Message}
                    onChange={handleChange}
                ></textarea>
                <h3>Enter email:</h3>
                <input type="text" id="UserEmail" value={formData.UserEmail} onChange={handleChange} />
                <br />
                <button type="submit">Create Ticket</button>
            </form>
        </div>
    );
}

export default CreateTicket;