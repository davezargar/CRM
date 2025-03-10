import { useEffect, useState, useContext, createContext } from "react";

export default ChangePassword;

function ChangePassword() {
    const [checkPassword, setCheckPassword] = useState("");
    const [password, setPassword] = useState("");


    return <div>
        <form className="formContainer">
            <div className="formRow">
                <label>New Password: <input type="text" name="password1" required value={password}
                    onChange={(e) => setCheckPassword(e.target.value)}></input></label>
            </div>

            <div className="formRow">
                <label>Repeat New Password: <input type="text" name="password2" required value={password}
                    onChange={(e) => setPassword(e.target.value)}></input></label>
            </div>

            <div className="submitContainer">
                <button type="submit" id="sendButton">Send</button>
            </div>
        </form>
    </div>
}