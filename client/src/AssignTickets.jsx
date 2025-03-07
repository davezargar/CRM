import { useState, useEffect } from "react";
import "./style/assignTickets.css";
import { CreateCategory} from "./CreateCategory.jsx";

export function AssignTickets() {
    const [workers, setWorkers] = useState([]);
    const [categories, setCategories] = useState([]);
    const [assignments, setAssignments] = useState({});
    const [selectedCategory, setSelectedCategory] = useState(null);

    useEffect(() => {
        async function fetchData() {
            try {
                const workersResponse = await fetch("/api/workers");
                const categoriesResponse = await fetch("/api/categories");
                const assignmentsResponse = await fetch("/api/assign-tickets");

                if (!workersResponse.ok || !categoriesResponse.ok || !assignmentsResponse.ok) {
                    throw new Error("Failed to fetch data");
                }

                const workersData = await workersResponse.json();
                const categoriesData = await categoriesResponse.json();
                const assignmentsData = await assignmentsResponse.json();

                setWorkers(workersData);
                setCategories(categoriesData);
                setAssignments(assignmentsData);
            } catch (error) {
                console.error(error);
            }
        }
        fetchData();
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
                {categories.map((category) => (
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
                                    {categories.find((cat) => cat.id === categoryId)?.name}
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
        
                    