import "./style/CustomerServicePanel.css"

export default function CustomerServicePanel() {
    return ReplyButtonDisplay();
}

export function ReplyButtonDisplay() {
    return <div className="replyContainer"><button id="replyButton">Reply</button></div>

}