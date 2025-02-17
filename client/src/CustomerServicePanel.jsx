import "./style/CustomerServicePanel.css";
import { useEffect, useState } from "react";

export default function CustomerServicePanel() {
    const [showMailWindow, setShowMailWindow] = useState(false)
    return (
        <div>
            {!showMailWindow && <ReplyButtonDisplay onClick={() => setShowMailWindow(true)} />}
            {showMailWindow && <DisplayMailWindow onClose={() => setShowMailWindow(false)} />}
        </div>
    )
}

export function ReplyButtonDisplay({ onClick }) {
    return <div className="replyContainer">
        <button id="replyButton" onClick={onClick}>Reply</button>
    </div>

}



function DisplayMailWindow({ onClose }) {

    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("")

    async function handleSubmit(e) {
        e.preventDefault();

        try {
            const response = await fetch("/api/sendMessage", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ title, description })
            });
            if (!response) {
                throw new Error("Response failed");
            }
            alert("Successfully sent the message to the endpoint");
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
                <input type="checkbox" id="checkResolveTickets" name="resolveTickets" value="check"></input>
                <label for="resolveTickets">Resolve Ticket</label>
                <button type="submit" id="sendButton">Send</button>
            </div>

        </form>
    </div>
}