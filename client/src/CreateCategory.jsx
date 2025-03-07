import { useState } from "react";

export function CreateCategory({ onCategoryAdded }) {
    const [categoryName, setCategoryName] = useState("");
    const [companyId, setCompanyId] = useState("");

    const handleSubmit = async (event) => {
        event.preventDefault();
        if (!categoryName.trim()) return alert("Category name cannot be empty.");
        if (!companyId.trim() || isNaN(companyId)) return alert("Company ID must be a valid number.");

        try {
            console.log("Sending request to API with:", categoryName, "Company ID:", companyId); //for debugging 

            const response = await fetch("/api/ticket-categories", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ name: categoryName, companyId: parseInt(companyId) }),
            });

            console.log("Response Status:", response.status);
            const responseData = await response.json();
            console.log("Response Data:", responseData);

            if (!response.ok) throw new Error("Failed to create category");

            alert("Category added!");
            setCategoryName("");
            setCompanyId("");
            onCategoryAdded();
        } catch (error) {
            console.error("Error adding category:", error);
        }
    };


    return (
        <div className="create-category">
            <h2>Create New Category</h2>
            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Category Name"
                    value={categoryName}
                    onChange={(e) => setCategoryName(e.target.value)}
                />
                <input
                    type="number"
                    placeholder="Company ID"
                    value={companyId}
                    onChange={(e) => setCompanyId(e.target.value)}
                />
                <button type="submit">Add Category</button>
            </form>
        </div>
    );
}