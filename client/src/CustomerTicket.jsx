import { Outlet, useParams } from "react-router";
import { useState, useEffect } from "react";
import MessageBox from './MessageBox';


export default CustomerTicket;

function CustomerTicket() {
    const [Ticket, SetTicket] = useState([]);
    let params = useParams();
    let ticketId = params.ticketId;
    const [Refresh, SetRefresh] = useState(false);

    const [Messages, SetMessages] = useState([]);
    const [FeedbackForm, setFeedbackForm] = useState(false);

    useEffect(() => {
        fetch(`/api/tickets/${ticketId}`)
            .then(response => response.json())
            .then(data => {
                console.log(data);
                SetMessages(data.messages);
                SetTicket(data.ticketRecord);
            });
    }, [Refresh]);

    function datetimeFormatter(datetime) {
        const date = new Date(Date.parse(datetime));
        const formattedDate = date.toLocaleString('en-GB', { timeZoneName: 'short' });
        return formattedDate;
    }

    function checkStatus(Ticket) {
        if (Ticket.timeClosed == null) {
            return <p>Status: Ongoing</p>
        } else {
            return <p>Status: Resolved</p>
        }
    }

    function handleSubmitFeedback(event) {
        event.preventDefault();
        const formData = new FormData(event.target);
        const feedbackData = {
            rating: formData.get("rating"),
            comment: formData.get("comment"),
            ticketId
        };

        fetch("/api/feedback", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(feedbackData)
        })
        .then(response => {
            if (!response.ok) throw new Error("Failed to submit feedback");
            alert("Thank you for your feedback!");
            setFeedbackForm(false);
        })
        .catch(error => {
            console.error(error);
            alert("Could not submit feedback");
        });
    }

    return <div id={"ticket-detailed"}>
        <div className="info">
            <ul id={"information"}>
                <li key={Ticket.ticketId}>
                    <p>From: {Ticket.userFk} </p> {checkStatus(Ticket)}
                    <p>Created: {datetimeFormatter(Ticket.timePosted)} </p>
                </li>
            </ul>
        </div>
        <div className="refreshContainer">
            <button className="refreshButton" onClick={() => SetRefresh(current => !current)}>Refresh</button>
        </div>
        <ul id={"ticketList"}>
            <li key={Ticket.ticketId}>
                <p className={"ticketId"}>{Ticket.ticketId}</p>
                <p className={"category"}>{Ticket.category}</p>
                <p className={"subcategory"}>{Ticket.subcategory}</p>
                <p className={"title"}>{Ticket.title}</p>
                <p className={"timeposted"}>{datetimeFormatter(Ticket.timePosted)}</p>
            </li>
        </ul>

        {Ticket.timeClosed && (
            <div className="feedbackContainer">
                <button className="feedbackBtn" onClick={() => setFeedbackForm(true)}>Give Feedback</button>
            </div>
        )}

        {showFeedbackPopup && (
            <div className="popup">
                <div className="popup-content">
                    <h3>Provide Feedback</h3>
                    <form onSubmit={handleSubmitFeedback}>
                        <label>
                            Rating:
                            <select name="rating" required>
                                <option value="5">★★★★★</option>
                                <option value="4">★★★★☆</option>
                                <option value="3">★★★☆☆</option>
                                <option value="2">★★☆☆☆</option>
                                <option value="1">★☆☆☆☆</option>
                            </select>
                        </label>

                        <label>
                            Comments:
                            <textarea name="comment" rows="4" required></textarea>
                        </label>

                        <button type="submit">Submit Feedback</button>
                        <button type="button" onClick={() => setFeedbackForm(false)}>Cancel</button>
                    </form>
                </div>
            </div>
        )}

        <div className="messageContainer">
            <ul id={"messageList"}>
                {Messages.map((Message =>
                    <li key={Message.messageId}>
                        <p className={"time_Posted"}>Time sent: {datetimeFormatter(Message.timePosted)}</p>
                        <h3>Title:<p className={"title"}>{Message.title}</p></h3>
                        <h4>Message:</h4>
                        <p className={"message"}>{Message.message}</p>
                        <h5>From:<p className={"messageSender"}>{Message.userId}</p></h5>
                    </li>
                ))}
            </ul>
        </div>
        <MessageBox ticket_Id={ticketId}></MessageBox>
    </div>
}

function MessageList({ Messages }) {


    return <ul id={"messageList"}>
        {Messages.map((Message =>
            <li key={Message.messageId}>
                <p className={"time_Posted"}>{Message.timePosted} </p>
                <p className={"messageId"}>{Message.messageId}</p>
                <p className={"title"}>{Message.title}</p>
                <p className={"message"}>{Message.message}</p>
                <p className={"messageSender"}>{Message.userId}</p>
            </li>
        ))}
    </ul>
}