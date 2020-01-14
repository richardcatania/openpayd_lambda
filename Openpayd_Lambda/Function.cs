using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.EC2;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Openpayd_Lambda
{
    public class Function
    {
        public void FunctionHandler(ILambdaContext context)
        {
            foreach (Amazon.RegionEndpoint regionEndPoint in Amazon.RegionEndpoint.EnumerableAllRegions)
            {
                CollectRegionData(regionEndPoint);
            }
        }

        private void CollectRegionData(Amazon.RegionEndpoint regionEndpoint)
        {
            var client = new AmazonEC2Client(regionEndpoint);

            try
            {

                var vpcs = client.DescribeVpcs().Vpcs.ToList();
                var subnets = client.DescribeSubnets().Subnets.ToList();

                foreach (var vpc in vpcs)
                {
                    SaveVpcToDB(regionEndpoint, vpc);
                }

                foreach (var subnet in subnets)
                {
                    SaveSubnetToDB(regionEndpoint, subnet);
                }
            }
            catch (Exception e)
            {

            }
        }

        private void SaveVpcToDB(Amazon.RegionEndpoint regionEndpoint, Amazon.EC2.Model.Vpc vpc)
        {
            try
            {

                var client = new AmazonDynamoDBClient(regionEndpoint);
                var table = Table.LoadTable(client, "Vpc");
                var jsonData = JsonConvert.SerializeObject(vpc);

                table.PutItem(Document.FromJson(jsonData));
            }
            catch (Amazon.DynamoDBv2.Model.ResourceNotFoundException e)
            {
                Console.WriteLine("No VPCs found in {0}", regionEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SaveSubnetToDB(Amazon.RegionEndpoint regionEndpoint, Amazon.EC2.Model.Subnet subnet)
        {
            try
            {
                var client = new AmazonDynamoDBClient(regionEndpoint);
                var table = Table.LoadTable(client, "Subnet");
                var jsonData = JsonConvert.SerializeObject(subnet);

                table.PutItem(Document.FromJson(jsonData));
            }
            catch (Amazon.DynamoDBv2.Model.ResourceNotFoundException e)
            {
                Console.WriteLine("No Subnets found in {0}", regionEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
