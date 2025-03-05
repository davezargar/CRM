import { useState, useEffect } from "react";
import "./style/assignTickets.css";

export function AssignTickets() {
    const [workers, setWorkers] = useState([]);
    const [categories, setCategories] = useState([]);
    const [assignments, setAssignments] = useState({});

    useEffect(() => {
        async function fetchData() {
            try 
            {
                const workersResponse = await fetch("/api/workers");
                const categoriesResponse = await fetch("/api/categories");

                if (!workersResponse.ok || !categoriesResponse.ok) 
                {
                    throw new Error("Failed to fetch data");
                }

                const workersData = await workersResponse.json();
                const categoriesData = await categoriesResponse.json();

                setWorkers(workersData);
                setCategories(categoriesData);
                setAssignments(workersData.reduce((acc, worker) => ({ ...acc, [worker.email]: [] }), {}));
            } catch (error) 
            {
                console.error(error);
            }
        }
        fetchData();
    }, []);

    const handleDragStart = (event, categoryId) => 
    {
        event.dataTransfer.setData("categoryId", categoryId);
    };

    const handleDrop = (event, workerEmail) => 
    {
        event.preventDefault();
        const categoryId = parseInt(event.dataTransfer.getData("category"));
        setAssignments((prev) => ({
            ...prev,
            [workerEmail]: [...new Set([...prev[workerEmail], category])], // Avoid duplicates
        }));
    };

    const handleDragOver = (event) => 
    {
        event.preventDefault();
    };

    const handleSaveAssignments = async () => 
    {
        try 
        {
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
            <div className="categories">
                {categories.map((category) => (
                    <div
                        key={category.id}
                        className="category"
                        draggable
                        onDragStart={(event) => handleDragStart(event, category.name)}
                    >
                        {category.name}
                    </div>
                ))}
            </div>
            <div className="workers">
                {workers.map((worker) => (
                    <div
                        key={worker.email}
                        className="worker"
                        onDrop={(event) => handleDrop(event, worker.email)}
                        onDragOver={handleDragOver}
                    >
                        <h3>{worker.email}</h3>
                        <div className="assigned-categories">
                            {assignments[worker.email]?.map((cat, idx) => (
                                <span key={idx} className="assigned-category">{cat}</span>
                            ))}
                        </div>
                    </div>
                ))}
            </div>
            <button onClick={handleSaveAssignments}>Save Assignments</button>
        </div>
    );
}

        
                    