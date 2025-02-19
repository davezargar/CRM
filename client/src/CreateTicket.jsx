import {NavLink} from "react-router";
import { useState } from "react";
import "./style/CreateTicket.css"
function CreateTicket() {
    const [formData, setFormData] = useState({
    Category: "",
    Subcategory: "",
    Title: "",
    Message: "",
    User_fk:"",
    Company_fk: 1, //Behöver fixa automatisk company&user koppling
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
    <h3>title</h3>
    <input type="text" id="Title" value={formData.Title} onChange={handleChange}/>
    <h3>Message</h3>
    <textarea type="text" id="Message" rows={12} cols={100} value={formData.Message} onChange={handleChange}></textarea>
    <h3>Enter email:</h3>
    <input type="text" id="User_fk" value={formData.User_fk} onChange={handleChange} />
    <br />
    <button type="submit">Create Ticket</button>
    </form>
    </div>
    
}

export default CreateTicket;