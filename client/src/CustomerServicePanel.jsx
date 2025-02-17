import "./style/CustomerServicePanel.css"

export default function CustomerServicePanel() {
    return DisplayMailWindow();
}

export function ReplyButtonDisplay() {
    return <div className="replyContainer"><button id="replyButton" onClick={DisplayMailWindow}>Reply</button></div>

}

function DisplayMailWindow() {
    return <div className="mainContainer">
        <form className="mailContainer">

            <div className="formRow">
                <label>Title: <input type="text" name="title" required value={Text}></input></label>
            </div>

            <div className="formRow">
                <label htmlFor="desc">Description:</label>
                <textarea name="desc" required rows="4"></textarea>

            </div>

            <div className="checkboxRow">
                <input type="checkbox" id="checkResolveTickets" name="resolveTickets" value="check"></input>
                <label for="resolveTickets">Resolve Ticket</label>
                <button type="submit" id="sendButton">Send</button>
            </div>

        </form>
    </div>
}