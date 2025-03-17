import { useState } from "react";
import "./style/FeedbackForm.css";
function FeedbackForm () {
    const token = new URLSearchParams(window.location.search).get('token');
    const [feedbackData, setFeedbackData] = useState({
        rating: "",
        comment: "",
    });

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFeedbackData((prevData) => ({
            ...prevData,
            [name]: value,
        }));
    };
    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            console.log(feedbackData);
            console.log(JSON.stringify({feedbackData, token}));
            const response = await fetch("/api/feedback-form", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({feedbackData, token}),
            });
            if (!response.ok) {
                throw new Error("Something went wrong when creating review");
            }
            alert("request sent");
        } catch (error) {
            console.error(error);
            alert("Couldn't create review.");
        }
    }
        return <div id="reviewForm">
    <form className="reviewContainer" onSubmit={handleSubmit}>
        <h3>Rating</h3>
    <select id="reviewSelect" name="rating" required value={feedbackData.Rating} onChange={handleChange}>
        <option value="" disabled>Give 1-5 stars</option>
        <option value="1">★☆☆☆☆</option>
        <option value="2">★★☆☆☆</option>
        <option value="3">★★★☆☆</option>
        <option value="4">★★★★☆</option>
        <option value="5">★★★★★</option>
    </select>
    <h3>Comments</h3>
    <label id="reviewLabel">How was your experience?<br></br>
        Please elaborate
        <input id="reviewInput" type="text" name="comment" value={feedbackData.Comments} onChange={handleChange}></input></label>
    <div className="submitContainer">
                <button type="submit" id="sendButton">Send</button>
            </div>
    </form>
</div>

}

export default FeedbackForm;