import { useState, useEffect } from "react";
import "./style/assignTickets.css";
import { CreateCategory} from "./CreateCategory.jsx";

export function AssignTickets() {
    const [workers, setWorkers] = useState([]);
    const [ticketCategories, setTicketCategories] = useState([]);
    const [assignments, setAssignments] = useState({});
    const [selectedCategory, setSelectedCategory] = useState(null);

    useEffect(() => {
        async function fetchAssignments() {
            try {
                const response = await fetch("/api/assign-tickets", { credentials: "include" });

                if (!response.ok) {
                    throw new Error("Failed to fetch assigned categories");
                }

                const data = await response.json();
                console.log("Fetched Assigned Categories:", data); 
                setAssignments(data);
            } catch (error) {
                console.error("Error fetching assigned categories:", error);
            }
        }
        fetchAssignments();
    }, []);

    useEffect(() => {
        async function fetchCategories() {
            try {
                const response = await fetch("/api/ticket-categories", { credentials: "include" });

                if (!response.ok) {
                    throw new Error("Failed to fetch ticket categories");
                }

                const data = await response.json();
                console.log("Fetched Ticket Categories:", data); 
                setTicketCategories(data);
            } catch (error) {
                console.error("Error fetching ticket categories:", error);
            }
        }
        fetchCategories();
    }, []);

    useEffect(() => {
        async function fetchWorkers() {
            try {
                const response = await fetch("/api/workers", { credentials: "include" });

                if (!response.ok) {
                    throw new Error("Failed to fetch workers");
                }

                const data = await response.json();
                console.log("Fetched Workers:", data); 
                setWorkers(data);
            } catch (error) {
                console.error("Error fetching workers:", error);
            }
        }
        fetchWorkers();
    }, []);

    const handleDragStart = (event, categoryId) => {
        event.dataTransfer.setData("categoryId", categoryId);
    };

    const handleDrop = (event, workerEmail) => {
        event.preventDefault();
        const categoryId = parseInt(event.dataTransfer.getData("categoryId"));
        setAssignments((prev) => ({
            ...prev,
            [workerEmail]: [...new Set([...(prev[workerEmail] || []), categoryId])],
        }));
    };

    const handleDragOver = (event) => event.preventDefault();

    const handleCategoryClick = (workerEmail, categoryId) => {
        if (selectedCategory?.workerEmail === workerEmail && selectedCategory?.categoryId === categoryId) {
            
            setAssignments((prev) => ({
                ...prev,
                [workerEmail]: prev[workerEmail].filter((id) => id !== categoryId),
            }));
            setSelectedCategory(null);
        } else {
            
            setSelectedCategory({ workerEmail, categoryId });
        }
    };

    const handleSaveAssignments = async () => {
        try {
            const response = await fetch("/api/assign-tickets", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(assignments),
            });
            if (!response.ok) throw new Error("Failed to save assignments");
            alert("Assignments saved!");
        } catch (error) {
            console.error(error);
        }
    };

    return (
        <div className="assign-tickets-container">
            <h1>Assign Tickets</h1>
            <CreateCategory onCategoryAdded={() => window.location.reload()} />
            <div className="categories">
                <h2>Drag Categories:</h2>
                {ticketCategories.map((category) => (
                    <div
                        key={category.id}
                        className="category"
                        draggable
                        onDragStart={(event) => handleDragStart(event, category.id)}
                    >
                        {category.name}
                    </div>
                ))}
            </div>
            <div className="workers">
                <h2>Assign to Workers:</h2>
                {workers.map((worker) => (
                    <div
                        key={worker.email}
                        className="worker"
                        onDrop={(event) => handleDrop(event, worker.email)}
                        onDragOver={handleDragOver}
                    >
                        <h3>{worker.email}</h3>
                        <div className="assigned-categories">
                            {(assignments[worker.email] || []).map((categoryId, idx) => (
                                <span
                                    key={idx}
                                    className={`assigned-category ${
                                        selectedCategory?.workerEmail === worker.email && selectedCategory?.categoryId === categoryId
                                            ? "selected"
                                            : ""
                                    }`}
                                    onClick={() => handleCategoryClick(worker.email, categoryId)}
                                >
                                    {ticketCategories.find((cat) => cat.id === categoryId)?.name}
                                </span>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
            <button onClick={handleSaveAssignments}>Save Assignments</button>
        </div>
    );
}
        
                    