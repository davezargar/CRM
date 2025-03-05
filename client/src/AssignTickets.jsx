import { useState, useEffect } from "react";
import "./style/assignTickets.css";

export function AssignTickets() {
    const [workers, setWorkers] = useState([]);
    const [categories, setCategories] = useState([]);
    const [assignments, setAssignments] = useState([]);
    
    useEffect(() => {
        async function fetchData(){
            try{
                const workers = await fetch("/api/workers");
                const categories = await fetch("/api/categories");
                
                if (!workersResponse.ok || !categoriesResponse.ok) {
                    
                    throw new Error("failed to fetch data");
                }
                
                const workersData = await workersResponse.json();
                const categoriesData = await categoriesResponse.json();
                
                setWorkers(workersData);
                setCategories(categoriesData);
                setAssignments(workersData.reduce((acc, worker) => ({ ...acc, [worker.email]: [] }), {}));
            } catch (error) {
                console.error(error);
            }
        }
        fetchData();
    }, []);
    
    const handleDragStart = (event, category) => {
        event.dataTransfer.setData("category", category);
    };
    
    const handleDragEnd = (event, workerEmail) => {
        event.preventDefault();
        const category = event.target.dataTransfer.getData("category");
        setAssignments((prev) => ({
            ...prev,
            [workerEmail]: 
                [...new Set([...prev[workerEmail], category])],
        }));
    };
    
    const handDragOver = (event) => 
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
            if (!response.ok) throw new Error("Failed to add save assignments");
            alert("assignments added successfully.");
            } catch (error) {
            console.error(error);
        }
    }