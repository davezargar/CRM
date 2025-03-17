import { NavLink } from "react-router";
import { useState, useEffect } from "react";
import "./style/CreateTicket.css";
import * as React from "react";

export function CreateTicket() {
    return <div></div>
}

/*
export function CreateTicket() {
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
*/
export function CreateIkeaTicket(){
    const [formData, setFormData] = useState({
        CategoryName: "",
        SubcategoryName: "",
        Title: "",
        Message: "",
        UserEmail: "",
        CompanyFk: 1, //set company for current form
    });

    
    
    function getCategories(){
        
    }
    
    const [categories, setCategories] = useState([]);
    const [subcategories, setSubcategories] = useState([]);

    useEffect(()=>{
        fetch(`/api/form/categories/${formData.CompanyFk}`)
            .then(response => response.json())
            .then(data => {
                formData.CategoryName = data[0].mainCategory;
                setSubcategories(data[0].subcategories);
                formData.SubcategoryName = subcategories[0];
                setCategories(data);
            });
    }, []);

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

    
    useEffect(()=>{
        for(let i = 0; i < categories.length; i++)
        {
            if(categories[i].mainCategory === formData.CategoryName)
            {
                setSubcategories(categories[i].subcategories);
            }
        }
        console.log(formData)
    }, [formData]);
    

    const handleChange = (e) => {
        const { id, value } = e.target;
        setFormData((prevData) => ({
            ...prevData,
            [id]: value,
        }));
    };
    
    
    const formClassName = formData.CompanyFk === 1? "ikea-form" : "default-form";

    return (
        <div id="createTicket" className={formClassName}>
            <h1>Create Ticket</h1>
            <form id="formId" onSubmit={handleSubmit}>
                <h3>Category (temp data)</h3>
                
                <select required id="CategoryName" value={formData.CategoryName} onChange={handleChange}>
                    {categories.map((category =>
                        <option key={category.mainCategory} value={category.mainCategory}>{category.mainCategory}</option>
                    ))}
                </select>
                <br />
                <h3>Subcategory (temp data)</h3>
                <select required id="SubcategoryName" value={formData.SubcategoryName} onChange={handleChange}>
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

export function CreateMikromjukTicket() {
    const [formData, setFormData] = useState({
        CategoryName: "",
        SubcategoryName: "",
        Title: "",
        Message: "",
        UserEmail: "",
        CompanyFk: 4, //set company for current form
    });


    function getCategories() {

    }

    const [categories, setCategories] = useState([]);
    const [subcategories, setSubcategories] = useState([]);

    useEffect(() => {
        fetch(`/api/form/categories/${formData.CompanyFk}`)
            .then(response => response.json())
            .then(data => {
                formData.CategoryName = data[0].mainCategory;
                setSubcategories(data[0].subcategories);
                formData.SubcategoryName = subcategories[0];
                setCategories(data);
            });
    }, []);

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


    useEffect(() => {
        for (let i = 0; i < categories.length; i++) {
            if (categories[i].mainCategory === formData.CategoryName) {
                setSubcategories(categories[i].subcategories);
            }
        }
        console.log(formData)
    }, [formData]);


    const handleChange = (e) => {
        const {id, value} = e.target;
        setFormData((prevData) => ({
            ...prevData,
            [id]: value,
        }));
    };

    return (
        <div id="createTicket">
            <h1>Create Ticket</h1>
            <form id="formId" onSubmit={handleSubmit}>
                <h3>Category (temp data)</h3>

                <select required id="CategoryName" value={formData.CategoryName} onChange={handleChange}>
                    {categories.map((category =>
                            <option key={category.mainCategory}
                                    value={category.mainCategory}>{category.mainCategory}</option>
                    ))}
                </select>
                <br/>
                <h3>Subcategory (temp data)</h3>
                <select required id="SubcategoryName" value={formData.SubcategoryName} onChange={handleChange}>
                    {subcategories.map((sub) => (
                        <option key={sub} value={sub}>
                            {sub}
                        </option>
                    ))}
                </select>
                <h3>Title</h3>
                <input type="text" id="Title" value={formData.Title} onChange={handleChange}/>
                <h3>Message</h3>
                <textarea
                    id="Message"
                    rows={12}
                    cols={100}
                    value={formData.Message}
                    onChange={handleChange}
                ></textarea>
                <h3>Enter email:</h3>
                <input type="text" id="UserEmail" value={formData.UserEmail} onChange={handleChange}/>
                <br/>
                <button type="submit">Create Ticket</button>
            </form>
        </div>
    );
}