using Solnet.Programs.Utilities;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solnet.Programs.Token2022
{
    /// <summary>
    /// Implements the Token 2022 Program methods.
    /// <remarks>
    /// For more information see:
    /// https://spl.solana.com/token
    /// https://docs.rs/spl-token-2022/latest/spl_token_2022/
    /// </remarks>
    /// </summary>
    public static class Token2022Program
    {
        /// <summary>
        /// The public key of the Token 2022 Program.
        /// </summary>
        public static readonly PublicKey ProgramIdKey = new("TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb");

        /// <summary>
        /// The program's name.
        /// </summary>
        private const string ProgramName = "Token 2022 Program";

        /// <summary>
        /// Initializes an instruction to transfer tokens from one account to another either directly or via a delegate.
        /// If this account is associated with the native mint then equal amounts of SOL and Tokens will be transferred to the destination account.
        /// </summary>
        /// <param name="source">The public key of the account to transfer tokens from.</param>
        /// <param name="destination">The public key of the account to account to transfer tokens to.</param>
        /// <param name="amount">The amount of tokens to transfer.</param>
        /// <param name="authority">The public key of the authority.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction Transfer(
            PublicKey source, PublicKey destination, ulong amount, PublicKey authority, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(source, false),
                AccountMeta.Writable(destination, false),
            };
            keys = AddSigners(keys, authority, signers);
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeTransferData(amount)
            };
        }

        /// <summary>
        /// <para>
        /// Initializes an instruction to transfer tokens from one account to another either directly or via a delegate.
        /// If this account is associated with the native mint then equal amounts of SOL and Tokens will be transferred to the destination account.
        /// </para>
        /// <para>
        /// This instruction differs from Transfer in that the token mint and decimals value is checked by the caller.
        /// This may be useful when creating transactions offline or within a hardware wallet.
        /// </para>
        /// </summary>
        /// <param name="source">The public key of the account to transfer tokens from.</param>
        /// <param name="destination">The public key of the account to account to transfer tokens to.</param>
        /// <param name="amount">The amount of tokens to transfer.</param>
        /// <param name="decimals">The token decimals.</param>
        /// <param name="authority">The public key of the authority account.</param>
        /// <param name="tokenMint">The public key of the token mint.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction TransferChecked(
            PublicKey source, PublicKey destination, ulong amount, int decimals, PublicKey authority, PublicKey tokenMint,
            IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(source, false),
                AccountMeta.ReadOnly(tokenMint, false),
                AccountMeta.Writable(destination, false),
            };
            keys = AddSigners(keys, authority, signers);
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeTransferCheckedData(amount, decimals)
            };
        }

        /// <summary>
        /// <para>Initializes an instruction to initialize a new account to hold tokens.
        /// If this account is associated with the native mint then the token balance of the initialized account will be equal to the amount of SOL in the account.
        /// If this account is associated with another mint, that mint must be initialized before this command can succeed.
        /// </para>
        /// <para>
        /// The InitializeAccount instruction requires no signers and MUST be included within the same Transaction
        /// as the system program's <see cref="SystemProgram.CreateAccount(PublicKey,PublicKey,ulong,ulong,PublicKey)"/>"/>
        /// instruction that creates the account being initialized.
        /// Otherwise another party can acquire ownership of the uninitialized account.
        /// </para>
        /// </summary>
        /// <param name="account">The public key of the account to initialize.</param>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="authority">The public key of the account to set as authority of the initialized account.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction InitializeAccount(PublicKey account, PublicKey mint, PublicKey authority)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false),
                AccountMeta.ReadOnly(mint, false),
                AccountMeta.ReadOnly(authority, false),
                AccountMeta.ReadOnly(SysVars.RentKey, false)
            };
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeInitializeAccountData()
            };
        }

        /// <summary>
        /// Initializes an instruction to initialize a multi signature token account.
        /// </summary>
        /// <param name="multiSignature">Public key of the multi signature account.</param>
        /// <param name="signers">Addresses of multi signature signers.</param>
        /// <param name="m">The number of signatures required to validate this multi signature account.</param>
        public static TransactionInstruction InitializeMultiSignature(PublicKey multiSignature,
            IEnumerable<PublicKey> signers, int m)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(multiSignature, false),
                AccountMeta.ReadOnly(SysVars.RentKey, false)
            };
            keys.AddRange(signers.Select(signer => AccountMeta.ReadOnly(signer, false)));
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeInitializeMultiSignatureData(m)
            };
        }

        /// <summary>
        /// Initializes an instruction to transfer tokens from one account to another either directly or via a delegate.
        /// If this account is associated with the native mint then equal amounts of SOL and Tokens will be transferred to the destination account.
        /// </summary>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="decimals">The token decimals.</param>
        /// <param name="mintAuthority">The public key of the token mint authority.</param>
        /// <param name="freezeAuthority">The token freeze authority.</param>
        public static TransactionInstruction InitializeMint(PublicKey mint, int decimals, PublicKey mintAuthority,
            PublicKey freezeAuthority = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(mint, false),
                AccountMeta.ReadOnly(SysVars.RentKey, false)
            };

            int freezeAuthorityOpt = freezeAuthority != null ? 1 : 0;
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeInitializeMintData(
                    mintAuthority,
                    freezeAuthority ?? new Account().PublicKey,
                    decimals,
                    freezeAuthorityOpt)
            };
        }

        /// <summary>
        /// Initializes an instruction to mint tokens to a destination account.
        /// </summary>
        /// <param name="mint">The public key token mint.</param>
        /// <param name="destination">The public key of the account to mint tokens to.</param>
        /// <param name="amount">The amount of tokens.</param>
        /// <param name="mintAuthority">The token mint authority account.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction MintTo(PublicKey mint, PublicKey destination, ulong amount,
            PublicKey mintAuthority, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(mint, false),
                AccountMeta.Writable(destination, false),
            };
            keys = AddSigners(keys, mintAuthority, signers);

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeMintToData(amount)
            };
        }

        /// <summary>
        /// Initializes an instruction to approve a transaction.
        /// </summary>
        /// <param name="source">The public key source account.</param>
        /// <param name="delegatePublicKey">The public key of the delegate account authorized to perform a transfer from the source account.</param>
        /// <param name="authority">The public key of the authority of the source account.</param>
        /// <param name="amount">The maximum amount of tokens the delegate may transfer.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction Approve(
            PublicKey source, PublicKey delegatePublicKey, PublicKey authority, ulong amount,
            IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(source, false),
                AccountMeta.ReadOnly(delegatePublicKey, false)
            };

            keys = AddSigners(keys, authority, signers);

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeApproveData(amount)
            };
        }

        /// <summary>
        /// Initializes an instruction to revoke a transaction.
        /// </summary>
        /// <param name="source">The public key source account.</param>
        /// <param name="authority">The public key of the authority of the source account.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction Revoke(PublicKey source, PublicKey authority,
            IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(source, false),
            };
            keys = AddSigners(keys, authority, signers);

            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeRevokeData()
            };
        }

        /// <summary>
        /// Initialize an instruction to set an authority on an account.
        /// </summary>
        /// <param name="account">The public key of the account to set the authority on.</param>
        /// <param name="authority">The type of authority to set.</param>
        /// <param name="currentAuthority">The public key of the current authority of the specified type.</param>
        /// <param name="newAuthority">The public key of the new authority.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction SetAuthority(
            PublicKey account, Models.Token2022Program.AuthorityType authority, PublicKey currentAuthority, PublicKey newAuthority = null,
            IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false),
            };
            keys = AddSigners(keys, currentAuthority, signers);

            int newAuthorityOpt = newAuthority != null ? 1 : 0;
            newAuthority ??= new Account().PublicKey;
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeSetAuthorityData(authority, newAuthorityOpt, newAuthority)
            };
        }

        /// <summary>
        /// Initialize an instruction to burn tokens.
        /// </summary>
        /// <param name="source">The public key of the account to burn tokens from.</param>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="amount">The amount of tokens to burn.</param>
        /// <param name="authority">The public key of the authority of the source account.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction Burn(PublicKey source, PublicKey mint, ulong amount, PublicKey authority,
            IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(source, false),
                AccountMeta.Writable(mint, false),
            };
            keys = AddSigners(keys, authority, signers);
            return new TransactionInstruction()
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeBurnData(amount)
            };
        }

        /// <summary>
        /// Initialize an instruction to close an account.
        /// </summary>
        /// <param name="account">The public key of the account to close.</param>
        /// <param name="destination">The public key of the account that will receive the SOL.</param>
        /// <param name="authority">The public key of the authority of the source account.</param>
        /// <param name="programId">The public key which represents the associated program id.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction CloseAccount(PublicKey account, PublicKey destination, PublicKey authority,
            PublicKey programId, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false),
                AccountMeta.Writable(destination, false),
            };
            keys = AddSigners(keys, authority, signers);
            return new TransactionInstruction()
            {
                ProgramId = programId.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeCloseAccountData()
            };
        }

        /// <summary>
        /// Initialize an instruction to freeze a token account.
        /// </summary>
        /// <param name="account">The public key of the account to freeze.</param>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="freezeAuthority">The public key of the authority of the freeze authority for the token mint.</param>
        /// <param name="programId">The public key which represents the associated program id.</param>
        /// <param name="signers">Signing accounts if the <c>freezeAuthority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction FreezeAccount(PublicKey account, PublicKey mint, PublicKey freezeAuthority,
            PublicKey programId, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false),
                AccountMeta.ReadOnly(mint, false),
            };
            keys = AddSigners(keys, freezeAuthority, signers);
            return new TransactionInstruction()
            {
                ProgramId = programId.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeFreezeAccountData()
            };
        }

        /// <summary>
        /// Initialize an instruction to thaw a token account.
        /// </summary>
        /// <param name="account">The public key of the account to thaw.</param>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="freezeAuthority">The public key of the freeze authority for the token mint.</param>
        /// <param name="programId">The public key which represents the associated program id.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction ThawAccount(PublicKey account, PublicKey mint, PublicKey freezeAuthority,
            PublicKey programId, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false),
                AccountMeta.ReadOnly(mint, false),
            };
            keys = AddSigners(keys, freezeAuthority, signers);
            return new TransactionInstruction()
            {
                ProgramId = programId.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeThawAccountData()
            };
        }

        /// <summary>
        /// Initialize an instruction to approve a transaction.
        /// <para>
        /// This instruction differs from Approve in that the amount and decimals value is checked by the caller.
        /// This may be useful when creating transactions offline or within a hardware wallet.
        /// </para>
        /// </summary>
        /// <param name="source">The public key of the source account.</param>
        /// <param name="delegatePublicKey">The public key of the delegate account authorized to perform a transfer from the source account.</param>
        /// <param name="authority">The public key of the authority of the source account.</param>
        /// <param name="amount">The maximum amount of tokens the delegate may transfer.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <param name="decimals">The token decimals.</param>
        /// <param name="mint">The public key of the token mint.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction ApproveChecked(
            PublicKey source, PublicKey delegatePublicKey, ulong amount, byte decimals, PublicKey authority, PublicKey mint,
            IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(source, false),
                AccountMeta.ReadOnly(mint, false),
                AccountMeta.ReadOnly(delegatePublicKey, false),
            };
            keys = AddSigners(keys, authority, signers);
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeApproveCheckedData(amount, decimals)
            };
        }

        /// <summary>
        /// Initialize an instruction to approve a transaction.
        /// <para>
        /// This instruction differs from MintTo in that the amount and decimals value is checked by the caller.
        /// This may be useful when creating transactions offline or within a hardware wallet.
        /// </para>
        /// </summary>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="destination">The public key of the account to mint tokens to.</param>
        /// <param name="mintAuthority">The public key of the token's mint authority account.</param>
        /// <param name="amount">The amount of tokens.</param>
        /// <param name="decimals">The token decimals.</param>
        /// <param name="signers">Signing accounts if the <c>mintAuthority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction MintToChecked(PublicKey mint, PublicKey destination,
            PublicKey mintAuthority, ulong amount, int decimals, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(mint, false),
                AccountMeta.Writable(destination, false),
            };
            keys = AddSigners(keys, mintAuthority, signers);
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeMintToCheckedData(amount, decimals)
            };
        }

        /// <summary>
        /// Initialize an instruction to burn tokens.
        /// <para>
        /// This instruction differs from Burn in that the amount and decimals value is checked by the caller.
        /// This may be useful when creating transactions offline or within a hardware wallet.
        /// </para>
        /// </summary>
        /// <param name="mint">The public key of the token mint.</param>
        /// <param name="account">The public key of the account to burn from.</param>
        /// <param name="authority">The public key of the authority of the source account.</param>
        /// <param name="amount">The amount of tokens.</param>
        /// <param name="decimals">The token decimals.</param>
        /// <param name="signers">Signing accounts if the <c>authority</c> is a multi signature.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction BurnChecked(PublicKey mint, PublicKey account, PublicKey authority,
            ulong amount, int decimals, IEnumerable<PublicKey> signers = null)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false),
                AccountMeta.Writable(mint, false),
            };
            keys = AddSigners(keys, authority, signers);
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeBurnCheckedData(amount, decimals)
            };
        }

        /// <summary>
        /// Initialize an instruction to sync native tokens.
        /// </summary>
        /// <param name="account">The public key of the token account.</param>
        /// <returns>The transaction instruction.</returns>
        public static TransactionInstruction SyncNative(PublicKey account)
        {
            List<AccountMeta> keys = new()
            {
                AccountMeta.Writable(account, false)
            };
            return new TransactionInstruction
            {
                ProgramId = ProgramIdKey.KeyBytes,
                Keys = keys,
                Data = Token2022ProgramData.EncodeSyncNativeData()
            };
        }

        /// <summary>
        /// Adds the list of signers to the list of keys.
        /// </summary>
        /// <param name="keys">The instruction's list of keys.</param>
        /// <param name="authority">The public key of the authority account.</param>
        /// <param name="signers">The list of signers.</param>
        /// <returns>The list of keys with the added signers.</returns>
        private static List<AccountMeta> AddSigners(List<AccountMeta> keys, PublicKey authority = null,
            IEnumerable<PublicKey> signers = null)
        {
            if (signers != null)
            {
                keys.Add(AccountMeta.ReadOnly(authority, false));
                keys.AddRange(signers.Select(signer => AccountMeta.ReadOnly(signer, true)));
            }
            else
            {
                keys.Add(AccountMeta.ReadOnly(authority, true));
            }

            return keys;
        }

        /// <summary>
        /// Decodes an instruction created by the System Program.
        /// </summary>
        /// <param name="data">The instruction data to decode.</param>
        /// <param name="keys">The account keys present in the transaction.</param>
        /// <param name="keyIndices">The indices of the account keys for the instruction as they appear in the transaction.</param>
        /// <returns>A decoded instruction.</returns>
        public static DecodedInstruction Decode(ReadOnlySpan<byte> data, IList<PublicKey> keys, byte[] keyIndices)
        {
            byte instruction = data.GetU8(Token2022ProgramData.MethodOffset);

            if(!Enum.IsDefined(typeof(TokenProgramInstructions.Values), instruction))
            {
                return new()
                {
                    PublicKey = ProgramIdKey,
                    InstructionName = "Unknown Instruction",
                    ProgramName = ProgramName,
                    Values = new Dictionary<string, object>(),
                    InnerInstructions = new List<DecodedInstruction>()
                };
            }

            TokenProgramInstructions.Values instructionValue = (TokenProgramInstructions.Values)instruction;

            DecodedInstruction decodedInstruction = new()
            {
                PublicKey = ProgramIdKey,
                InstructionName = TokenProgramInstructions.Names[instructionValue],
                ProgramName = ProgramName,
                Values = new Dictionary<string, object>(),
                InnerInstructions = new List<DecodedInstruction>()
            };

            switch (instructionValue)
            {
                case TokenProgramInstructions.Values.InitializeMint:
                    Token2022ProgramData.DecodeInitializeMintData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeAccount:
                    Token2022ProgramData.DecodeInitializeAccountData(decodedInstruction, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeMultiSignature:
                    Token2022ProgramData.DecodeInitializeMultiSignatureData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.Transfer:
                    Token2022ProgramData.DecodeTransferData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.Approve:
                    Token2022ProgramData.DecodeApproveData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.Revoke:
                    Token2022ProgramData.DecodeRevokeData(decodedInstruction, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.SetAuthority:
                    Token2022ProgramData.DecodeSetAuthorityData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.MintTo:
                    Token2022ProgramData.DecodeMintToData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.Burn:
                    Token2022ProgramData.DecodeBurnData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.CloseAccount:
                    Token2022ProgramData.DecodeCloseAccountData(decodedInstruction, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.FreezeAccount:
                    Token2022ProgramData.DecodeFreezeAccountData(decodedInstruction, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.ThawAccount:
                    Token2022ProgramData.DecodeThawAccountData(decodedInstruction, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.TransferChecked:
                    Token2022ProgramData.DecodeTransferCheckedData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.ApproveChecked:
                    Token2022ProgramData.DecodeApproveCheckedData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.MintToChecked:
                    Token2022ProgramData.DecodeMintToCheckedData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.BurnChecked:
                    Token2022ProgramData.DecodeBurnCheckedData(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.SyncNative:
                    Token2022ProgramData.DecodeSyncNativeData(decodedInstruction, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeAccount2:
                    Token2022ProgramData.DecodeInitializeAccount2(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeAccount3:
                    Token2022ProgramData.DecodeInitializeAccount3(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeMint2:
                    Token2022ProgramData.DecodeInitializeMint2(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeMultiSignature2:
                    Token2022ProgramData.DecodeInitializeMultiSignature2(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.GetAccountDataSize:
                    Token2022ProgramData.DecodeGetAccountDataSize(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.InitializeImmutableOwner:
                    Token2022ProgramData.DecodeInitializeImmutableOwner(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.AmountToUiAmount:
                    Token2022ProgramData.DecodeAmountToUiAmount(decodedInstruction, data, keys, keyIndices);
                    break;
                case TokenProgramInstructions.Values.UiAmountToAmount:
                    Token2022ProgramData.DecodeUiAmountToAmount(decodedInstruction, data, keys, keyIndices);
                    break;
            }
            return decodedInstruction;
        }
    }
}