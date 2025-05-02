namespace Solnet.Programs.Models.Token2022Program
{
    /// <summary>
    /// Represents the types of authorities for <see cref="Token2022.Token2022Program.SetAuthority"/> instructions.
    /// </summary>
    public enum AuthorityType : byte
    {
        /// <summary>
        /// Authority to mint new tokens.
        /// </summary>
        MintTokens = 0,

        /// <summary>
        /// Authority to freeze any account associated with the mint.
        /// </summary>
        FreezeAccount = 1,

        /// <summary>
        /// Owner of a given account token.
        /// </summary>
        AccountOwner = 2,

        /// <summary>
        /// Authority to close a given account.
        /// </summary>
        CloseAccount = 3,

        /// <summary>
        /// Authority to set the transfer fee.
        /// </summary>
        TransferFeeConfig = 4,

        /// <summary>
        /// Authority to withdraw withheld tokens from a mint.
        /// </summary>
        WithheldWithdraw = 5,

        /// <summary>
        /// Authority to close a mint account.
        /// </summary>
        CloseMint = 6,

        /// <summary>
        /// Authority to set the interest rate.
        /// </summary>
        InterestRate = 7,

        /// <summary>
        /// Authority to transfer or burn any tokens for a mint.
        /// </summary>
        PermanentDelegate = 8,

        /// <summary>
        /// Authority to update confidential transfer mint and approve accounts for confidential transfers.
        /// </summary>
        ConfidentialTransferMint = 9,

        /// <summary>
        /// Authority to set the transfer hook program id.
        /// </summary>
        TransferHookProgramId = 10,

        /// <summary>
        /// Authority to set the withdraw withheld authority encryption key.
        /// </summary>
        ConfidentialTransferFeeConfig = 11,

        /// <summary>
        /// Authority to set the metadata address.
        /// </summary>
        MetadataPointer = 12,

        /// <summary>
        /// Authority to set the group address.
        /// </summary>
        GroupPointer = 13,

        /// <summary>
        /// Authority to set the group member address.
        /// </summary>
        GroupMemberPointer = 14,

        /// <summary>
        /// Authority to set the UI amount scale.
        /// </summary>
        ScaledUiAmount = 15,

        /// <summary>
        /// Authority to pause or resume minting / transferring / burning.
        /// </summary>
        Pause = 16
    }
}