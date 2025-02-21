import "./style/MessageBox.css";
import { useEffect, useState, useContext, createContext } from "react";

const TicketContext = createContext();

export default function MessageBox({ ticket_Id }) {
    const [showMailWindow, setShowMailWindow] = useState(false);


    return (
        <TicketContext.Provider value={ticket_Id}>
            <div>
                {!showMailWindow && <ReplyButtonDisplay onClick={() => setShowMailWindow(true)} />}
                {showMailWindow && <DisplayMailWindow onClose={() => setShowMailWindow(false)} />}
            </div>
        </TicketContext.Provider>
    )
}

export function ReplyButtonDisplay({ onClick }) {
    return <div className="replyContainer">
        <button id="replyButton" onClick={onClick}>Reply</button>
    </div>

}



function DisplayMailWindow({ onClose }) {

    const [title, setTitle] = useState(() => localStorage.getItem("title") || (""));
    const [description, setDescription] = useState(() => localStorage.getItem("description") || (""));
    const [isChecked, setIsChecked] = useState(false);
    const ticket_id = useContext(TicketContext);

    const handleCheckboxChange = (e) => {
        setIsChecked(e.target.checked)
    }



    useEffect(() => {
        localStorage.setItem("title", title);
    }, [title]);

    useEffect(() => {
        localStorage.setItem("description", description)
    }, [description]);

    async function handleSubmit(e) {
        e.preventDefault();

        try {
            if (isChecked == true) {
                const response = await fetch("/api/tickets", {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ resolved: isChecked, ticket_id })
                });
                if (!response) {
                    throw new Error("Response failed");
                }
                localStorage.removeItem("title");
                localStorage.removeItem("description");

                setTitle("");
                setDescription("");
            }

            const response = await fetch("/api/messages", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ title, description, ticket_id_fk: ticket_id })

            });
            if (!response) {
                throw new Error("Response failed");
            }
            console.log("Sending ticket_id: ", ticket_id);
            alert("Successfully sent the message to the endpoint");
            localStorage.removeItem("title");
            localStorage.removeItem("description");

            setTitle("");
            setDescription("");

        } catch (error) {
            console.error(error);
            alert("couldnt send the form")
        }
    }

    return <div className="mainContainer">


        <form className="mailContainer" onSubmit={handleSubmit}>

            <div className="formRow">
                <button className="closeButton" onClick={onClose}>X</button>
                <label>Title: <input type="text" name="title" required value={title}
                    onChange={(e) => setTitle(e.target.value)} ></input></label>
            </div>

            <div className="formRow">
                <label htmlFor="desc-id">Description:</label>
                <textarea id="desc-id" name="desc" required rows="4" value={description}
                    onChange={(e) => setDescription(e.target.value)}></textarea>
            </div>

            <div className="checkboxRow">
                <input type="checkbox" checked={isChecked} onChange={handleCheckboxChange} id="checkResolveTickets" name="resolveTickets" value="check"></input>
                <label>Resolve Ticket</label>
                <button type="submit" id="sendButton">Send</button>
            </div>

        </form>
    </div>
}