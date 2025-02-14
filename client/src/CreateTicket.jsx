import {NavLink} from "react-router";
import { useState } from "react";
import "./style/CreateTicket.css"
function CreateTicket() {
    const [formData, setFormData] = useState({
    Category: "",
    Subcategory: "",
    Title: "",
    User_fk: "",
    Response_email: "",
    Company_fk: 2, //Behöver fixa automatisk company&user koppling
    });

    const handleChange = (e) => {
        setFormData({
          ...formData,
          [e.target.id]: e.target.value,
        });
      };
      
    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const response = await fetch("/api/CreateTicket", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(formData),
            });
            if (!response.ok) {
                throw new Error("Something went wrong when creating ticket");
            }
            alert("Ticket created");
        } catch (error) {
            console.error(error);
            alert("Couldn't create ticket.");
        }
    }
    return <div id="createTicket"><h1>Create Ticket</h1>
    <form id="formId" onSubmit={handleSubmit}>
    <h3>Category</h3>
    <input type="text" id="Category" value={formData.Category} onChange={handleChange} />
    <h3>Subcategory</h3>
    <input type="text" id="Subcategory" value={formData.Subcategory} onChange={handleChange} />
    <h3>Description</h3>
    <textarea id="Title" rows={12} cols={100} value={formData.description} onChange={handleChange}></textarea>
    <h3>Enter your email:</h3>
    <input type="text" id="User_fk" value={formData.User_fk} onChange={handleChange} />
    <h3>Enter your response email:</h3>
    <input type="email" id="Response_email" value={formData.Response_email} onChange={handleChange} />
    <br />
    <button type="submit">Create Ticket</button>
    </form>
    </div>
    
}

export default CreateTicket;