import { useEffect, useState, useContext, createContext } from "react";
import "./style/changePassword.css"

export default ChangePassword;

function ChangePassword() {
    const [checkPassword, setCheckPassword] = useState("");
    const [password, setPassword] = useState("");
    const token = new URLSearchParams(window.location.search).get('token');
    const [message, setMessage] = useState("");

    const handleSubmit = async (e) => {
        e.preventDefault();

        if (checkPassword !== password) {
            setCheckPassword("");
            setPassword("");
            setMessage("❌ The passwords don't match each other... Please try again");
            setTimeout(() => setMessage(""), 3000);
            return;
        }

        try {
            const response = await fetch("/api/workers/password", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ password, token }),
            });
            if (!response.ok) {
                throw new Error("Something went wrong with updating password");
            }
            setMessage("✅ Password changed successfully!");
            setTimeout(() => setMessage(""), 10000);
        } catch (error) {
            console.error(error);
            setMessage("❌  Password couldn't be updated!");
            setTimeout(() => setMessage(""), 10000);
        }
    }


    return <div>
        <form className="formContainer" onSubmit={handleSubmit}>
            <div className="formRow">
                <label>New Password: </label><input type="text" name="password1" required value={checkPassword}
                    onChange={(e) => setCheckPassword(e.target.value)}></input>
            </div>

            <div className="formRow">
                <label>Repeat New Password: </label><input type="text" name="password2" required value={password}
                    onChange={(e) => setPassword(e.target.value)}></input>
            </div>

            <div className="submitContainer1">
                <button type="submit" id="sendButton">Confirm</button>
                {message && <p className="message-password">{message}</p>}
            </div>
        </form>
    </div>
}