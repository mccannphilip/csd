using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using weka;
using weka.core;
using weka.core.converters;

namespace MLOpenInterface
{
    public class MLReplyModel
    {
        static weka.classifiers.meta.FilteredClassifier smo = null;
        static weka.classifiers.meta.FilteredClassifier j48 = null;
        static weka.classifiers.meta.FilteredClassifier nb = null;
        static weka.classifiers.meta.FilteredClassifier knn = null;
        static Instances test = null;
 
        public bool InitialiseSmo(string modelPath)
        {
            if (smo == null)
            {
                string basePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                string modelFile = Path.Combine(basePath, modelPath);
                smo = (weka.classifiers.meta.FilteredClassifier)SerializationHelper.read(modelFile);
            }
            return (smo != null);
        }

        public bool InitialiseJ48(string modelPath)
        {
            if (j48 == null)
            {
                string basePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                string modelFile = Path.Combine(basePath, modelPath);
                j48 = (weka.classifiers.meta.FilteredClassifier)SerializationHelper.read(modelFile);
            }
            return (j48 != null);
        }

        public bool InitialiseNb(string modelPath)
        {
            if (nb == null)
            {
                string basePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                string modelFile = Path.Combine(basePath, modelPath);
                nb = (weka.classifiers.meta.FilteredClassifier)SerializationHelper.read(modelFile);
            }
            return (nb != null);
        }

        public bool InitialiseKnn(string modelPath)
        {
            if (knn == null)
            {
                string basePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                string modelFile = Path.Combine(basePath, modelPath);
                knn = (weka.classifiers.meta.FilteredClassifier)SerializationHelper.read(modelFile);
            }
            return (knn != null);
        }

        public double MakePrediction(string dataPath, wsFeature.WSFeature feat)
        {
            string basePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

            string dataFile = Path.Combine(basePath, dataPath);
            ConverterUtils.DataSource source = new ConverterUtils.DataSource(dataFile);
            test = source.getDataSet();
            test.setClassIndex(9);

            /*
            w5 integer
            recipientcount integer
            to string
            subject string
            text string
            textlength integer
            subjectlength integer
            mailimportance integer
            attachmentcount integer
            @@class@@ {Reply,NoReply}
             */

            Instance x = test.instance(0);
            x.setValue(0, feat.W5);
            x.setValue(1, feat.Length);
            x.setValue(2, feat.Mailbox);
            x.setValue(3, feat.Subject);
            x.setValue(4, feat.MessageCleanTagged);
            x.setValue(5, feat.Length);
            x.setValue(6, feat.Subject.Length);
            x.setValue(7, 5);
            x.setValue(8, 0);

            double[] smoValues = smo.distributionForInstance(x);
            double[] j48Values = j48.distributionForInstance(x);
            double[] nbValues = nb.distributionForInstance(x);
            double[] knnValues = knn.distributionForInstance(x);

            // NoReply prediction contributes positively to the ensemble score
            // Reply prediction contributes negatively to the ensemble score
            double smoValue = smoValues[1] - smoValues[0];
            double j48Value = j48Values[1] - j48Values[0];
            double nbValue = nbValues[1] - nbValues[0];
            double knnValue = knnValues[1] - knnValues[0];
            double ensembleScore =smoValue + j48Value + nbValue + knnValue;
            return ensembleScore;
        }
    }
}