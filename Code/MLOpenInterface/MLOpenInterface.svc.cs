using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MLOpenInterface
{
    public class MLOpenInterface : IMLOpenInterface
    {
        #region Correct format methods only

        private MLSpamModel ensembleSpamModel = new MLSpamModel();
        private MLReplyModel ensembleReplyModel = new MLReplyModel();

        /// <summary>
        /// Input array:
        ///     [0]: From address
        ///     [1]: To address
        ///     [2]: Subject
        ///     [3]: Plain Text Body
        ///     [4]: HTML Text Body
        /// Output array:
        ///     [0]: Skillset
        ///     [1]: Priority
        ///     [2]: Auto-Close
        ///     [3]: CustomFieldName - "EnsembleScore"
        ///     [4]: CustomFieldValue - EnsembleScore
        /// </summary>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        public string[] MLRouting(string[] inputParameters)
        {
            string[] outputParameters = new string[5];

            string toAddress = string.Empty;
            string ccAddress = string.Empty;
            string subject = string.Empty;
            string textBody = string.Empty;
            string skillset = string.Empty;
            string scoreCustomField = string.Empty;

            string smoSpamModelFile = "model\\fc_smo_spam_model_ngram.model";
            string j48SpamModelFile = "model\\fc_j48_spam_model_ngram.model";
            string nbSpamModelFile = "model\\fc_nb_spam_model_ngram.model";
            string knnSpamModelFile = "model\\fc_ibk_spam_model_ngram.model";
            string spamDataFile = "model\\init_spam.arff";

            string smoReplyModelFile = "model\\fc_smo_noreply_model_ngram.model";
            string j48ReplyModelFile = "model\\fc_j48_noreply_model_ngram.model";
            string nbReplyModelFile = "model\\fc_nb_noreply_model_ngram.model";
            string knnReplyModelFile = "model\\fc_ibk_noreply_model_ngram.model";
            string replyDataFile = "model\\init_reply.arff";

            double spamDetectionThreshold = Properties.Settings.Default.SpamPredictionEnsembleThreshold;
            double replyPredictionThreshold = Properties.Settings.Default.ReplyPredictionEnsembleThreshold;

            ensembleSpamModel.InitialiseSmo(smoSpamModelFile);
            ensembleSpamModel.InitialiseJ48(j48SpamModelFile);
            ensembleSpamModel.InitialiseNb(nbSpamModelFile);
            ensembleSpamModel.InitialiseKnn(knnSpamModelFile);

            ensembleReplyModel.InitialiseSmo(smoReplyModelFile);
            ensembleReplyModel.InitialiseJ48(j48ReplyModelFile);
            ensembleReplyModel.InitialiseNb(nbReplyModelFile);
            ensembleReplyModel.InitialiseKnn(knnReplyModelFile);

            try
            {
                if (inputParameters != null)
                {
                    if (inputParameters[0] != null)
                    {
                        toAddress = inputParameters[0];
                    }
                    if (inputParameters[1] != null)
                    {
                        ccAddress = inputParameters[1];
                    }
                    if (inputParameters[2] != null)
                    {
                        subject = inputParameters[2];
                    }
                    if (inputParameters[3] != null)
                    {
                        textBody = inputParameters[3];
                    }
                    if (inputParameters[4] != null)
                    {
                        skillset = inputParameters[4];
                    }
                }

                // Convert these data properties to the form needed for the prediction model
                wsFeature.CSD x = new wsFeature.CSD();
                wsFeature.WSFeature feat = x.GetFeature(toAddress, ccAddress, subject, textBody);

                // Check SPAM
                double spamResult = ensembleSpamModel.MakePrediction(spamDataFile, feat);

                // Ensemble threshold
                if (spamResult > spamDetectionThreshold)
                {
                    outputParameters[0] = "EM_Spam";
                    outputParameters[1] = "10";
                    outputParameters[2] = "1";
                    outputParameters[3] = "MLScore";
                    outputParameters[4] = string.Format("{0:0.000}", spamResult);
                    return outputParameters;
                }

                // Check Reply / NoReply
                double noreplyResult = ensembleReplyModel.MakePrediction(replyDataFile, feat);

                // Ensemble threshold = 1
                if (noreplyResult > replyPredictionThreshold)
                {
                    outputParameters[0] = null;
                    outputParameters[1] = "10";
                    outputParameters[2] = "0";
                    outputParameters[3] = "MLScore";
                    outputParameters[4] = string.Format("{0:0.000}, {1:0.000}", spamResult, noreplyResult);
                    return outputParameters;
                }

                outputParameters[0] = null;
                outputParameters[1] = null;
                outputParameters[2] = null;
                outputParameters[3] = "MLScore";
                outputParameters[4] = string.Format("{0:0.000}, {1:0.000}", spamResult, noreplyResult);
            }
            catch (Exception ex)
            {
                outputParameters[0] = ex.Message;
                outputParameters[1] = ex.Source;
                outputParameters[2] = null;
                outputParameters[3] = null;
                outputParameters[4] = null;
            }

            return outputParameters;
        }
        #endregion
    }
}
